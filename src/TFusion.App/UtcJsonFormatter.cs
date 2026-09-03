using System.Globalization;
using System.Text.Json;
using Serilog.Events;
using Serilog.Formatting;

namespace TFusion.App;

internal sealed class UtcJsonFormatter : ITextFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
    };

    public void Format(LogEvent logEvent, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(output);

        var record = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["timestampUtc"] = logEvent.Timestamp.UtcDateTime.ToString("O", CultureInfo.InvariantCulture),
            ["level"] = logEvent.Level.ToString(),
            ["messageTemplate"] = logEvent.MessageTemplate.Text,
            ["message"] = logEvent.RenderMessage(CultureInfo.InvariantCulture),
        };

        if (logEvent.Exception is not null)
        {
            record["exception"] = logEvent.Exception.ToString();
        }

        foreach (var property in logEvent.Properties.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            record[property.Key] = ConvertValue(property.Value);
        }

        output.Write(JsonSerializer.Serialize(record, JsonOptions));
        output.WriteLine();
    }

    private static object? ConvertValue(LogEventPropertyValue value) => value switch
    {
        ScalarValue scalar => scalar.Value,
        SequenceValue sequence => sequence.Elements.Select(ConvertValue).ToArray(),
        StructureValue structure => structure.Properties.ToDictionary(
            property => property.Name,
            property => ConvertValue(property.Value),
            StringComparer.Ordinal),
        DictionaryValue dictionary => dictionary.Elements.ToDictionary(
            pair => Convert.ToString(pair.Key.Value, CultureInfo.InvariantCulture) ?? string.Empty,
            pair => ConvertValue(pair.Value),
            StringComparer.Ordinal),
        _ => value.ToString(),
    };
}
