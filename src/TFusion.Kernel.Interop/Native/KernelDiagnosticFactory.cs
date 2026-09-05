using System.Runtime.InteropServices;
using System.Text.Json;
using TFusion.Foundation.Diagnostics;

namespace TFusion.Kernel.Interop.Native;

internal static class KernelDiagnosticFactory
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    internal static CadDiagnostic LoadFailure(Exception exception) => new(
        KernelDiagnosticCodes.NativeBridgeLoadFailed,
        DiagnosticSeverity.Error,
        "The native CAD kernel bridge could not be loaded.",
        exception.Message,
        [new("nativeBridgePath", NativeBridgeLoader.ExpectedPath)]);

    internal static CadDiagnostic InvalidManagedText(string detail) => new(
        KernelDiagnosticCodes.InvalidArgument,
        DiagnosticSeverity.Error,
        "The text supplied to the native CAD kernel is invalid.",
        detail);

    internal static CadDiagnostic FromStatus(NativeStatus status, ulong contextHandle, string operation)
    {
        var diagnosticContext = IsContextUsable(status) ? contextHandle : 0;
        try
        {
            var readStatus = NativeText.Read(
                (byte[]? buffer, uint size, out uint required) => NativeMethods.ContextGetTextUtf8(
                    diagnosticContext,
                    NativeTextField.LastDiagnosticJson,
                    buffer,
                    size,
                    out required),
                out var json);
            if (readStatus == NativeStatus.Success)
            {
                var native = JsonSerializer.Deserialize<NativeDiagnostic>(json, JsonOptions);
                if (native is not null && !string.IsNullOrWhiteSpace(native.UserMessage))
                {
                    return new CadDiagnostic(
                        CodeFor(status, native.Code),
                        ParseSeverity(native.Severity),
                        native.UserMessage,
                        native.TechnicalMessage,
                        BuildContext(operation, native));
                }
            }
        }
        catch (Exception exception) when (IsInteropException(exception) || exception is JsonException)
        {
            return new CadDiagnostic(
                CodeFor(status, null),
                DiagnosticSeverity.Error,
                "The native CAD kernel operation failed.",
                $"{operation} returned {status}; its detailed diagnostic could not be read: {exception.Message}");
        }

        return new CadDiagnostic(
            CodeFor(status, null),
            DiagnosticSeverity.Error,
            "The native CAD kernel operation failed.",
            $"{operation} returned {status} without a readable detailed diagnostic.");
    }

    internal static bool IsInteropException(Exception exception) =>
        exception is DllNotFoundException
            or BadImageFormatException
            or EntryPointNotFoundException
            or MarshalDirectiveException;

    private static bool IsContextUsable(NativeStatus status) => status is not (
        NativeStatus.InvalidHandle
            or NativeStatus.StaleHandle
            or NativeStatus.TypeMismatch
            or NativeStatus.ContextMismatch);

    private static IEnumerable<KeyValuePair<string, string>> BuildContext(
        string fallbackOperation,
        NativeDiagnostic native)
    {
        yield return new("operation", string.IsNullOrWhiteSpace(native.Operation) ? fallbackOperation : native.Operation);
        yield return new("nativeStatus", native.Status ?? string.Empty);
        if (!string.IsNullOrWhiteSpace(native.NativeDetail))
        {
            yield return new("nativeDetail", native.NativeDetail);
        }
    }

    private static DiagnosticSeverity ParseSeverity(string? severity) => severity?.ToLowerInvariant() switch
    {
        "information" => DiagnosticSeverity.Information,
        "warning" => DiagnosticSeverity.Warning,
        "fatal" => DiagnosticSeverity.Fatal,
        _ => DiagnosticSeverity.Error,
    };

    private static DiagnosticCode CodeFor(NativeStatus status, string? reportedCode)
    {
        if (!string.IsNullOrWhiteSpace(reportedCode))
        {
            try
            {
                return new DiagnosticCode(reportedCode);
            }
            catch (ArgumentException)
            {
                // Fall through to the status-owned stable code.
            }
        }

        return status switch
        {
            NativeStatus.InvalidArgument => KernelDiagnosticCodes.InvalidArgument,
            NativeStatus.InvalidHandle => KernelDiagnosticCodes.InvalidHandle,
            NativeStatus.StaleHandle => KernelDiagnosticCodes.StaleHandle,
            NativeStatus.TypeMismatch => KernelDiagnosticCodes.TypeMismatch,
            NativeStatus.ContextMismatch => KernelDiagnosticCodes.ContextMismatch,
            NativeStatus.BufferTooSmall => KernelDiagnosticCodes.BufferTooSmall,
            NativeStatus.VersionMismatch => KernelDiagnosticCodes.VersionMismatch,
            NativeStatus.KernelError => KernelDiagnosticCodes.KernelError,
            NativeStatus.Unsupported => KernelDiagnosticCodes.Unsupported,
            _ => KernelDiagnosticCodes.InternalError,
        };
    }

    private sealed record NativeDiagnostic(
        string? Status,
        string? Code,
        string? Severity,
        string UserMessage,
        string? TechnicalMessage,
        string? Operation,
        string? NativeDetail);
}
