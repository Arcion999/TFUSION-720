using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using TFusion.Foundation.Diagnostics;
using TFusion.Kernel.Interop.Native;

namespace TFusion.Kernel.Tests;

public sealed class NativeContractTests
{
    [Fact]
    public void M2Context02RejectsInvalidAndRepeatedDestroy()
    {
        Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.ContextDestroy(0));
        Assert.Equal(NativeStatus.InvalidHandle, NativeMethods.ContextDestroy(0x0123_4567_89AB_CDEF));

        using var context = KernelBridge.CreateContext("stale-context").Value;
        var handle = context.NativeHandle;
        context.Dispose();
        Assert.Equal(NativeStatus.StaleHandle, NativeMethods.ContextDestroy(handle));
    }

    [Fact]
    public void M2Handle01RejectsWrongTypeWrongContextStaleAndDoubleRelease()
    {
        using var first = KernelBridge.CreateContext("first").Value;
        using var second = KernelBridge.CreateContext("second").Value;
        var probe = first.CreateRuntimeProbe().Value;
        var probeHandle = probe.NativeHandle;

        Assert.Equal(NativeStatus.TypeMismatch, NativeMethods.ContextDestroy(probeHandle));
        Assert.Equal(NativeStatus.ContextMismatch, NativeMethods.ProbeRelease(second.NativeHandle, probeHandle));
        probe.Dispose();
        Assert.Equal(NativeStatus.StaleHandle, NativeMethods.ProbeRelease(first.NativeHandle, probeHandle));
    }

    [Fact]
    public void M2Handle02ContextShutdownInvalidatesOwnedHandles()
    {
        var first = KernelBridge.CreateContext("owner").Value;
        using var second = KernelBridge.CreateContext("observer").Value;
        var probe = first.CreateRuntimeProbe().Value;
        var firstHandle = first.NativeHandle;
        var probeHandle = probe.NativeHandle;

        first.SafeHandle.SetHandleAsInvalid();
        Assert.Equal(NativeStatus.Success, NativeMethods.ContextDestroy(firstHandle));
        Assert.Equal(
            NativeStatus.StaleHandle,
            NativeMethods.ProbeGetRuntimeVersionUtf8(second.NativeHandle, probeHandle, null, 0, out _));
        probe.Dispose();
        first.Dispose();
    }

    [Fact]
    public void M2Exception01ContainsNativeExceptions()
    {
        using var context = KernelBridge.CreateContext("exception-boundary").Value;
        var cases = new[]
        {
            (NativeExceptionProbe.Occt, KernelDiagnosticCodes.KernelError),
            (NativeExceptionProbe.Standard, KernelDiagnosticCodes.InternalError),
            (NativeExceptionProbe.Unknown, KernelDiagnosticCodes.InternalError),
        };

        foreach (var (probe, expectedCode) in cases)
        {
            var result = context.ExerciseExceptionBoundary(probe);
            Assert.True(result.IsFailure);
            Assert.Equal(expectedCode, result.Diagnostics.Single().Code);
            Assert.Contains("exception", result.Diagnostics.Single().TechnicalDetail!, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void M2Utf802RejectsMalformedUtf8AndReturnsJsonDiagnostic()
    {
        using var context = KernelBridge.CreateContext("utf8-failure").Value;
        var malformed = new byte[] { 0xC3, 0x28 };
        Assert.Equal(
            NativeStatus.InvalidArgument,
            NativeMethods.ContextSetClientNameUtf8(context.NativeHandle, malformed, (uint)malformed.Length));

        var json = ReadNativeText(context.NativeHandle, NativeTextField.LastDiagnosticJson);
        using var document = JsonDocument.Parse(json);
        Assert.Equal("INVALID_ARGUMENT", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("TFN-KRN-0001", document.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public void M2Utf803ReportsRequiredLengthAndTerminatesShortBuffer()
    {
        using var context = KernelBridge.CreateContext("buffer-contract").Value;
        var shortBuffer = new byte[] { 0x7F, 0x7F };
        var status = NativeMethods.ContextGetTextUtf8(
            context.NativeHandle,
            NativeTextField.RuntimeOcctVersion,
            shortBuffer,
            (uint)shortBuffer.Length,
            out var required);

        Assert.Equal(NativeStatus.BufferTooSmall, status);
        Assert.Equal(0, shortBuffer[0]);
        Assert.True(required > shortBuffer.Length);
    }

    [Fact]
    public void M2Struct01RejectsUnknownStructVersionsAndSizes()
    {
        var info = new NativeContextCreateInfo
        {
            StructSize = 1,
            StructVersion = 99,
            RequestedAbiVersion = 1,
        };
        Assert.Equal(NativeStatus.InvalidArgument, NativeMethods.ContextCreate(in info, out var handle));
        Assert.Equal(0UL, handle);
    }

    [Fact]
    public void M2Lifetime01ManagedResourcesReturnRegistryToBaseline()
    {
        var before = Snapshot();
        for (var iteration = 0; iteration < 100; iteration++)
        {
            using var context = KernelBridge.CreateContext("managed-lifetime").Value;
            using var probe = context.CreateRuntimeProbe().Value;
            Assert.Contains("8.0.1", probe.GetRuntimeOcctVersion().Value, StringComparison.Ordinal);
        }
        var after = Snapshot();

        Assert.Equal(before.ActiveContexts, after.ActiveContexts);
        Assert.Equal(before.ActiveProbes, after.ActiveProbes);
        Assert.Equal(200UL, after.TotalAllocations - before.TotalAllocations);
        Assert.Equal(200UL, after.TotalReleases - before.TotalReleases);
    }

    [Fact]
    public void M2Ownership02SafeHandleFinalizerIsAFallbackForAbandonedContext()
    {
        var before = Snapshot();
        var reference = CreateAbandonedContext();

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        Assert.False(reference.IsAlive);
        var after = Snapshot();
        Assert.Equal(before.ActiveContexts, after.ActiveContexts);
        Assert.Equal(before.ActiveProbes, after.ActiveProbes);
        Assert.Equal(1UL, after.TotalAllocations - before.TotalAllocations);
        Assert.Equal(1UL, after.TotalReleases - before.TotalReleases);
    }

    private static NativeAllocationSnapshot Snapshot()
    {
        var snapshot = new NativeAllocationSnapshot
        {
            StructSize = (uint)Marshal.SizeOf<NativeAllocationSnapshot>(),
            StructVersion = NativeMethods.AllocationSnapshotVersion,
        };
        Assert.Equal(NativeStatus.Success, NativeMethods.DebugGetAllocationSnapshot(ref snapshot));
        return snapshot;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateAbandonedContext()
    {
        var context = KernelBridge.CreateContext("finalizer-fallback").Value;
        return new WeakReference(context);
    }

    private static string ReadNativeText(ulong context, NativeTextField field)
    {
        Assert.Equal(
            NativeStatus.BufferTooSmall,
            NativeMethods.ContextGetTextUtf8(context, field, null, 0, out var required));
        var buffer = new byte[required];
        Assert.Equal(
            NativeStatus.Success,
            NativeMethods.ContextGetTextUtf8(context, field, buffer, required, out var returnedRequired));
        Assert.Equal(required, returnedRequired);
        Assert.Equal(0, buffer[^1]);
        return Encoding.UTF8.GetString(buffer, 0, buffer.Length - 1);
    }
}
