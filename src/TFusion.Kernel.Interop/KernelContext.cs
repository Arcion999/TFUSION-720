using System.Runtime.InteropServices;
using System.Text;
using TFusion.Foundation.Results;
using TFusion.Kernel.Interop.Native;

namespace TFusion.Kernel.Interop;

public sealed class KernelContext : IDisposable
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly SafeKernelContextHandle handle;
    private bool disposed;

    private KernelContext(SafeKernelContextHandle handle) => this.handle = handle;

    internal ulong NativeHandle => handle.Value;

    internal SafeKernelContextHandle SafeHandle => handle;

    internal static Result<KernelContext> Create(string clientName)
    {
        ArgumentNullException.ThrowIfNull(clientName);
        byte[] nameBytes;
        try
        {
            nameBytes = StrictUtf8.GetBytes(clientName);
        }
        catch (EncoderFallbackException exception)
        {
            return Result.Failure<KernelContext>(KernelDiagnosticFactory.InvalidManagedText(exception.Message));
        }

        if (nameBytes.Length > 4096)
        {
            return Result.Failure<KernelContext>(KernelDiagnosticFactory.InvalidManagedText(
                "The UTF-8 client name exceeds the ABI v1 limit of 4096 bytes."));
        }

        GCHandle pin = default;
        try
        {
            var pointer = nint.Zero;
            if (nameBytes.Length > 0)
            {
                pin = GCHandle.Alloc(nameBytes, GCHandleType.Pinned);
                pointer = pin.AddrOfPinnedObject();
            }

            var createInfo = new NativeContextCreateInfo
            {
                StructSize = checked((uint)Marshal.SizeOf<NativeContextCreateInfo>()),
                StructVersion = NativeMethods.ContextCreateInfoVersion,
                RequestedAbiVersion = NativeMethods.AbiVersion,
                ClientNameUtf8 = pointer,
                ClientNameByteCount = checked((uint)nameBytes.Length),
            };
            var status = NativeMethods.ContextCreate(in createInfo, out var nativeHandle);
            if (status != NativeStatus.Success)
            {
                return Result.Failure<KernelContext>(KernelDiagnosticFactory.FromStatus(status, 0, nameof(Create)));
            }
            if (nativeHandle == 0)
            {
                return Result.Failure<KernelContext>(KernelDiagnosticFactory.FromStatus(
                    NativeStatus.InternalError,
                    0,
                    nameof(Create)));
            }

            return Result.Success(new KernelContext(SafeKernelContextHandle.Create(nativeHandle)));
        }
        catch (Exception exception) when (KernelDiagnosticFactory.IsInteropException(exception))
        {
            return Result.Failure<KernelContext>(KernelDiagnosticFactory.LoadFailure(exception));
        }
        finally
        {
            if (pin.IsAllocated)
            {
                pin.Free();
            }
        }
    }

    public Result<KernelBridgeInfo> GetBridgeInfo()
    {
        ThrowIfDisposed();
        var info = new NativeKernelInfo
        {
            StructSize = checked((uint)Marshal.SizeOf<NativeKernelInfo>()),
            StructVersion = NativeMethods.KernelInfoVersion,
        };
        var status = NativeMethods.ContextGetKernelInfo(NativeHandle, ref info);
        if (status != NativeStatus.Success)
        {
            return Result.Failure<KernelBridgeInfo>(KernelDiagnosticFactory.FromStatus(status, NativeHandle, nameof(GetBridgeInfo)));
        }

        var compiledResult = ReadText(NativeTextField.CompiledOcctVersion);
        if (compiledResult.IsFailure)
        {
            return Result.Failure<KernelBridgeInfo>(compiledResult.Diagnostics);
        }
        var runtimeResult = ReadText(NativeTextField.RuntimeOcctVersion);
        if (runtimeResult.IsFailure)
        {
            return Result.Failure<KernelBridgeInfo>(runtimeResult.Diagnostics);
        }
        if (info.CompiledOcctVersionRequiredBytes != StrictUtf8.GetByteCount(compiledResult.Value) + 1
            || info.RuntimeOcctVersionRequiredBytes != StrictUtf8.GetByteCount(runtimeResult.Value) + 1)
        {
            return Result.Failure<KernelBridgeInfo>(KernelDiagnosticFactory.FromStatus(
                NativeStatus.InternalError,
                NativeHandle,
                nameof(GetBridgeInfo)));
        }

        return Result.Success(new KernelBridgeInfo(
            info.AbiVersion,
            compiledResult.Value,
            runtimeResult.Value,
            info.Architecture == 1 ? "x64" : "unknown",
            NativeBridgeLoader.ExpectedPath));
    }

    public Result SetClientName(string value)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(value);
        byte[] bytes;
        try
        {
            bytes = StrictUtf8.GetBytes(value);
        }
        catch (EncoderFallbackException exception)
        {
            return Result.Failure(KernelDiagnosticFactory.InvalidManagedText(exception.Message));
        }
        if (bytes.Length > 4096)
        {
            return Result.Failure(KernelDiagnosticFactory.InvalidManagedText(
                "The UTF-8 client name exceeds the ABI v1 limit of 4096 bytes."));
        }

        var status = NativeMethods.ContextSetClientNameUtf8(
            NativeHandle,
            bytes.Length == 0 ? null : bytes,
            checked((uint)bytes.Length));
        return status == NativeStatus.Success
            ? Result.Success()
            : Result.Failure(KernelDiagnosticFactory.FromStatus(status, NativeHandle, nameof(SetClientName)));
    }

    public Result<string> GetClientName()
    {
        ThrowIfDisposed();
        return ReadText(NativeTextField.ContextClientName);
    }

    public Result<KernelRuntimeProbe> CreateRuntimeProbe()
    {
        ThrowIfDisposed();
        var status = NativeMethods.ProbeCreate(NativeHandle, out var probeHandle);
        return status == NativeStatus.Success && probeHandle != 0
            ? Result.Success(new KernelRuntimeProbe(SafeKernelProbeHandle.Create(handle, probeHandle), this))
            : Result.Failure<KernelRuntimeProbe>(KernelDiagnosticFactory.FromStatus(
                status == NativeStatus.Success ? NativeStatus.InternalError : status,
                NativeHandle,
                nameof(CreateRuntimeProbe)));
    }

    internal Result ExerciseExceptionBoundary(NativeExceptionProbe probe)
    {
        ThrowIfDisposed();
        var status = NativeMethods.TestExceptionBoundary(NativeHandle, probe);
        return status == NativeStatus.Success
            ? Result.Success()
            : Result.Failure(KernelDiagnosticFactory.FromStatus(status, NativeHandle, nameof(ExerciseExceptionBoundary)));
    }

    internal Result<string> ReadText(NativeTextField field)
    {
        var status = NativeText.Read(
            (byte[]? buffer, uint size, out uint required) => NativeMethods.ContextGetTextUtf8(
                NativeHandle,
                field,
                buffer,
                size,
                out required),
            out var value);
        return status == NativeStatus.Success
            ? Result.Success(value)
            : Result.Failure<string>(KernelDiagnosticFactory.FromStatus(status, NativeHandle, nameof(ReadText)));
    }

    public void Dispose()
    {
        if (!disposed)
        {
            handle.Dispose();
            disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(disposed, this);
}
