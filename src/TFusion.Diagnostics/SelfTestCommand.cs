using System.Runtime.InteropServices;
using System.Text.Json;
using TFusion.Foundation;
using TFusion.Foundation.Lifecycle;
using TFusion.Foundation.Storage;
using TFusion.Kernel.Interop;

namespace TFusion.Diagnostics;

public sealed class SelfTestCommand
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static int Execute(TextWriter standardOutput, TextWriter standardError)
    {
        ArgumentNullException.ThrowIfNull(standardOutput);
        ArgumentNullException.ThrowIfNull(standardError);

        var checks = new List<SelfTestCheck>();
        var nativeKernel = NativeKernelReport.NotLoaded();
        var isolatedRoot = Path.Combine(
            Path.GetTempPath(),
            "TFUSION-720-self-test",
            Guid.NewGuid().ToString("N"));

        try
        {
            checks.Add(CheckPlatform());
            checks.Add(CheckArchitecture());
            checks.Add(CheckBuildMetadata());
            checks.AddRange(CheckStorageAndSentinel(isolatedRoot));
            checks.AddRange(CheckNativeKernel(out nativeKernel));
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or NotSupportedException
                or InvalidOperationException)
        {
            checks.Add(new SelfTestCheck("selfTestExecution", false, "A controlled self-test operation failed."));
            standardError.WriteLine(exception.Message);
        }
        finally
        {
            TryRemoveIsolatedRoot(isolatedRoot, standardError);
        }

        var passed = checks.Count > 0 && checks.All(check => check.Passed);
        standardOutput.WriteLine(JsonSerializer.Serialize(new SelfTestReport(
            SchemaVersion: 1,
            Status: passed ? "pass" : "fail",
            Product: "TFUSION-720",
            Build: BuildInfo.Current,
            Checks: checks,
            NativeKernel: nativeKernel,
            Capabilities: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["cadKernel"] = nativeKernel.InitializationResult == "success"
                    ? "occt-native-bridge-foundation"
                    : "unavailable",
                ["renderer"] = "not-implemented",
                ["formatProviders"] = "not-implemented",
            }), JsonOptions));

        return passed ? 0 : 1;
    }

    private static SelfTestCheck CheckPlatform()
    {
        var passed = OperatingSystem.IsWindowsVersionAtLeast(10, 0, 26100);
        return new SelfTestCheck(
            "windowsRuntime",
            passed,
            passed ? "Windows runtime is supported." : "Windows 11 build 26100 or newer is required.");
    }

    private static SelfTestCheck CheckArchitecture()
    {
        var passed = RuntimeInformation.ProcessArchitecture == Architecture.X64;
        return new SelfTestCheck(
            "processArchitecture",
            passed,
            passed ? "Process architecture is x64." : "An x64 process is required.");
    }

    private static SelfTestCheck CheckBuildMetadata()
    {
        var build = BuildInfo.Current;
        var passed = Version.TryParse(build.ProductVersion, out _)
            && !string.IsNullOrWhiteSpace(build.InformationalVersion);
        return new SelfTestCheck(
            "buildMetadata",
            passed,
            passed ? "Build metadata is readable." : "Build metadata is invalid.");
    }

    private static IEnumerable<SelfTestCheck> CheckStorageAndSentinel(string isolatedRoot)
    {
        var pathsResult = ProductPaths.FromRoot(isolatedRoot);
        if (pathsResult.IsFailure)
        {
            return [new SelfTestCheck("applicationStorage", false, pathsResult.Diagnostics[0].UserMessage)];
        }

        var paths = pathsResult.Value;
        var createResult = paths.EnsureCreated();
        if (createResult.IsFailure)
        {
            return [new SelfTestCheck("applicationStorage", false, createResult.Diagnostics[0].UserMessage)];
        }

        var sentinel = new StartupSentinel(paths.StartupSentinelFile);
        var sessionId = Guid.NewGuid();
        var startResult = sentinel.BeginSession(sessionId, DateTimeOffset.UtcNow);
        var cleanResult = startResult.IsSuccess
            ? sentinel.MarkClean(sessionId, DateTimeOffset.UtcNow)
            : null;

        return
        [
            new SelfTestCheck("applicationStorage", true, "Isolated application directories are writable."),
            new SelfTestCheck(
                "startupSentinel",
                startResult.IsSuccess && cleanResult?.IsSuccess == true,
                startResult.IsSuccess && cleanResult?.IsSuccess == true
                    ? "Startup sentinel round-trip succeeded."
                    : "Startup sentinel round-trip failed."),
        ];
    }

    private static IEnumerable<SelfTestCheck> CheckNativeKernel(out NativeKernelReport report)
    {
        var abi = KernelBridge.QueryAbiVersion();
        if (abi.IsFailure)
        {
            var diagnostic = abi.Diagnostics[0];
            report = NativeKernelReport.Failed("load-failed", diagnostic.Code.ToString());
            return
            [
                new SelfTestCheck(
                    "nativeBridgeLoad",
                    false,
                    diagnostic.UserMessage,
                    diagnostic.Code.ToString()),
            ];
        }

        var negotiation = KernelBridge.NegotiateAbiVersion(KernelBridge.SupportedAbiVersion);
        if (negotiation.IsFailure)
        {
            var diagnostic = negotiation.Diagnostics[0];
            report = NativeKernelReport.Failed("loaded", diagnostic.Code.ToString(), abi.Value);
            return
            [
                new SelfTestCheck("nativeBridgeLoad", true, "TFusion.Kernel.Native.dll loaded from the packaged output."),
                new SelfTestCheck("nativeAbiNegotiation", false, diagnostic.UserMessage, diagnostic.Code.ToString()),
            ];
        }

        var contextResult = KernelBridge.CreateContext("TFusion.Diagnostics");
        if (contextResult.IsFailure)
        {
            var diagnostic = contextResult.Diagnostics[0];
            report = NativeKernelReport.Failed("loaded", diagnostic.Code.ToString(), abi.Value);
            return
            [
                new SelfTestCheck("nativeBridgeLoad", true, "TFusion.Kernel.Native.dll loaded from the packaged output."),
                new SelfTestCheck("nativeBridgeInitialization", false, diagnostic.UserMessage, diagnostic.Code.ToString()),
            ];
        }

        using var context = contextResult.Value;
        var infoResult = context.GetBridgeInfo();
        if (infoResult.IsFailure)
        {
            var diagnostic = infoResult.Diagnostics[0];
            report = NativeKernelReport.Failed("loaded", diagnostic.Code.ToString(), abi.Value);
            return
            [
                new SelfTestCheck("nativeBridgeLoad", true, "TFusion.Kernel.Native.dll loaded from the packaged output."),
                new SelfTestCheck("nativeBridgeInitialization", false, diagnostic.UserMessage, diagnostic.Code.ToString()),
            ];
        }

        var probeCreateResult = context.CreateRuntimeProbe();
        if (probeCreateResult.IsFailure)
        {
            var diagnostic = probeCreateResult.Diagnostics[0];
            report = NativeKernelReport.Failed("loaded", diagnostic.Code.ToString(), abi.Value);
            return
            [
                new SelfTestCheck("nativeBridgeLoad", true, "TFusion.Kernel.Native.dll loaded from the packaged output."),
                new SelfTestCheck("nativeBridgeInitialization", false, diagnostic.UserMessage, diagnostic.Code.ToString()),
            ];
        }

        using var probe = probeCreateResult.Value;
        var probeResult = probe.GetRuntimeOcctVersion();
        var info = infoResult.Value;
        var actualOcctExecuted = probeResult.IsSuccess
            && string.Equals(probeResult.Value, info.RuntimeOcctVersion, StringComparison.Ordinal);
        var probeDiagnosticCode = actualOcctExecuted || probeResult.Diagnostics.Count == 0
            ? null
            : probeResult.Diagnostics[0].Code.ToString();
        report = new NativeKernelReport(
            "loaded",
            info.AbiVersion,
            info.CompiledOcctVersion,
            info.RuntimeOcctVersion,
            info.Architecture,
            actualOcctExecuted ? "success" : "failed",
            info.NativeBridgePath,
            probeDiagnosticCode);

        return
        [
            new SelfTestCheck("nativeBridgeLoad", true, "TFusion.Kernel.Native.dll loaded from the packaged output."),
            new SelfTestCheck("nativeAbiNegotiation", negotiation.Value == 1, $"TFUSION native ABI v{negotiation.Value} negotiated."),
            new SelfTestCheck(
                "occtRuntimeExecution",
                actualOcctExecuted,
                actualOcctExecuted
                    ? "The bridge executed the linked Open CASCADE runtime and returned matching version metadata."
                    : "The Open CASCADE runtime probe failed."),
            new SelfTestCheck(
                "nativeBridgeInitialization",
                actualOcctExecuted,
                actualOcctExecuted ? "Native kernel context initialization succeeded." : "Native kernel context initialization failed."),
        ];
    }

    private static void TryRemoveIsolatedRoot(string isolatedRoot, TextWriter standardError)
    {
        try
        {
            if (Directory.Exists(isolatedRoot))
            {
                Directory.Delete(isolatedRoot, recursive: true);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            standardError.WriteLine($"Self-test cleanup failed: {exception.Message}");
        }
    }

    private sealed record SelfTestReport(
        int SchemaVersion,
        string Status,
        string Product,
        BuildInfo Build,
        IReadOnlyList<SelfTestCheck> Checks,
        NativeKernelReport NativeKernel,
        IReadOnlyDictionary<string, string> Capabilities);

    private sealed record SelfTestCheck(string Name, bool Passed, string Message, string? Code = null);

    private sealed record NativeKernelReport(
        string LoadStatus,
        uint? AbiVersion,
        string? CompiledOcctVersion,
        string? RuntimeOcctVersion,
        string? Architecture,
        string InitializationResult,
        string? NativeBridgePath,
        string? DiagnosticCode)
    {
        internal static NativeKernelReport NotLoaded() => new(
            "not-attempted", null, null, null, null, "not-attempted", null, null);

        internal static NativeKernelReport Failed(
            string loadStatus,
            string diagnosticCode,
            uint? abiVersion = null) => new(
                loadStatus, abiVersion, null, null, null, "failed", null, diagnosticCode);
    }
}
