using System.Collections.ObjectModel;

namespace TFusion.Foundation.Diagnostics;

public sealed class CadDiagnostic
{
    public CadDiagnostic(
        DiagnosticCode code,
        DiagnosticSeverity severity,
        string userMessage,
        string? technicalDetail = null,
        IEnumerable<KeyValuePair<string, string>>? context = null,
        IEnumerable<CadDiagnostic>? causes = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userMessage);

        Code = code;
        Severity = severity;
        UserMessage = userMessage;
        TechnicalDetail = technicalDetail;

        var contextCopy = context is null
            ? new Dictionary<string, string>(StringComparer.Ordinal)
            : new Dictionary<string, string>(context, StringComparer.Ordinal);

        Context = new ReadOnlyDictionary<string, string>(contextCopy);
        Causes = Array.AsReadOnly(causes?.ToArray() ?? []);
    }

    public DiagnosticCode Code { get; }

    public DiagnosticSeverity Severity { get; }

    public string UserMessage { get; }

    public string? TechnicalDetail { get; }

    public IReadOnlyDictionary<string, string> Context { get; }

    public IReadOnlyList<CadDiagnostic> Causes { get; }

    public bool IsError => Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Fatal;
}
