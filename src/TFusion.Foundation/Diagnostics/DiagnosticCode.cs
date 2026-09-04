namespace TFusion.Foundation.Diagnostics;

public readonly record struct DiagnosticCode
{
    public DiagnosticCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > 64 || value.Any(character => !IsAllowed(character)))
        {
            throw new ArgumentException(
                "Diagnostic codes may contain only ASCII uppercase letters, digits, periods, underscores, and hyphens.",
                nameof(value));
        }

        Value = value;
    }

    public string Value { get; }

    public override string ToString() => Value;

    private static bool IsAllowed(char character) =>
        character is >= 'A' and <= 'Z'
        || character is >= '0' and <= '9'
        || character is '.' or '_' or '-';
}

public static class FoundationDiagnosticCodes
{
    public static readonly DiagnosticCode InvalidConfiguration = new("TFN-FND-0001");
    public static readonly DiagnosticCode StorageUnavailable = new("TFN-FND-0002");
    public static readonly DiagnosticCode StartupStateInvalid = new("TFN-FND-0003");
    public static readonly DiagnosticCode StartupStateWriteFailed = new("TFN-FND-0004");
    public static readonly DiagnosticCode StartupSessionMismatch = new("TFN-FND-0005");
    public static readonly DiagnosticCode UnsupportedPlatform = new("TFN-FND-0006");
}
