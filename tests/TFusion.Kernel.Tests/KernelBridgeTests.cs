using TFusion.Foundation.Diagnostics;

namespace TFusion.Kernel.Tests;

public sealed class KernelBridgeTests
{
    [Fact]
    public void M2Abi01ReportsAndNegotiatesVersionOne()
    {
        var query = KernelBridge.QueryAbiVersion();
        Assert.True(query.IsSuccess);
        Assert.Equal(1U, query.Value);

        var negotiation = KernelBridge.NegotiateAbiVersion(1);
        Assert.True(negotiation.IsSuccess);
        Assert.Equal(1U, negotiation.Value);
    }

    [Theory]
    [InlineData(0U)]
    [InlineData(2U)]
    [InlineData(uint.MaxValue)]
    public void M2Abi02RejectsUnsupportedVersions(uint requested)
    {
        var result = KernelBridge.NegotiateAbiVersion(requested);
        Assert.True(result.IsFailure);
        Assert.Equal(KernelDiagnosticCodes.VersionMismatch, result.Diagnostics.Single().Code);
    }

    [Fact]
    public void M2Occt01ReportsRealCompiledAndRuntimeOcct()
    {
        using var context = KernelBridge.CreateContext("managed-test").Value;
        var result = context.GetBridgeInfo();

        Assert.True(result.IsSuccess);
        Assert.Equal(1U, result.Value.AbiVersion);
        Assert.Equal("x64", result.Value.Architecture);
        Assert.Contains("8.0.1", result.Value.CompiledOcctVersion, StringComparison.Ordinal);
        Assert.Contains("8.0.1", result.Value.RuntimeOcctVersion, StringComparison.Ordinal);
        Assert.True(File.Exists(result.Value.NativeBridgePath));
        Assert.True(File.Exists(Path.Combine(AppContext.BaseDirectory, "TKernel.dll")));
    }

    [Fact]
    public void M2Utf801RoundTripsNonAsciiText()
    {
        using var context = KernelBridge.CreateContext("blå konstruksjon — 精密").Value;
        Assert.Equal("blå konstruksjon — 精密", context.GetClientName().Value);

        var update = context.SetClientName("mål Ø12 ±0,01 mm");
        Assert.True(update.IsSuccess);
        Assert.Equal("mål Ø12 ±0,01 mm", context.GetClientName().Value);
    }

    [Fact]
    public void M2Context01DisposeIsIdempotent()
    {
        var context = KernelBridge.CreateContext("dispose-test").Value;
        context.Dispose();
        context.Dispose();
        Assert.Throws<ObjectDisposedException>(() => context.GetBridgeInfo());
    }

    [Fact]
    public void M2Ownership01ProbeKeepsItsContextHandleAliveUntilProbeRelease()
    {
        var context = KernelBridge.CreateContext("safe-handle-owner").Value;
        var probe = context.CreateRuntimeProbe().Value;

        context.Dispose();
        var version = probe.GetRuntimeOcctVersion();
        Assert.True(version.IsSuccess);
        Assert.Contains("8.0.1", version.Value, StringComparison.Ordinal);

        probe.Dispose();
        probe.Dispose();
    }
}
