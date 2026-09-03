using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace TFusion.Foundation.Identifiers;

internal static class StrongGuid
{
    public static Guid Validate(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("A stable identifier cannot be empty.", parameterName);
        }

        return value;
    }

    public static Guid New() => Guid.NewGuid();

    public static Guid Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return Validate(Guid.ParseExact(value, "D"), nameof(value));
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? value,
        out Guid parsed)
    {
        if (Guid.TryParseExact(value, "D", out parsed) && parsed != Guid.Empty)
        {
            return true;
        }

        parsed = Guid.Empty;
        return false;
    }

    public static string Format(Guid value) => value.ToString("D", CultureInfo.InvariantCulture);
}
