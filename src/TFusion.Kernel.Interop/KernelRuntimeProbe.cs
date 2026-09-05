using TFusion.Foundation.Results;
using TFusion.Kernel.Interop.Native;

namespace TFusion.Kernel.Interop;

public sealed class KernelRuntimeProbe : IDisposable
{
    private readonly SafeKernelProbeHandle handle;
    private readonly KernelContext context;
    private bool disposed;

    internal KernelRuntimeProbe(SafeKernelProbeHandle handle, KernelContext context)
    {
        this.handle = handle;
        this.context = context;
    }

    internal ulong NativeHandle => handle.Value;

    public Result<string> GetRuntimeOcctVersion()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        try
        {
            var status = NativeText.Read(
                (byte[]? buffer, uint size, out uint required) => NativeMethods.ProbeGetRuntimeVersionUtf8(
                    context.NativeHandle,
                    NativeHandle,
                    buffer,
                    size,
                    out required),
                out var value);
            return status == NativeStatus.Success
                ? Result.Success(value)
                : Result.Failure<string>(KernelDiagnosticFactory.FromStatus(
                    status,
                    context.NativeHandle,
                    nameof(GetRuntimeOcctVersion)));
        }
        catch (Exception exception) when (KernelDiagnosticFactory.IsInteropException(exception))
        {
            return Result.Failure<string>(KernelDiagnosticFactory.LoadFailure(exception));
        }
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
}
