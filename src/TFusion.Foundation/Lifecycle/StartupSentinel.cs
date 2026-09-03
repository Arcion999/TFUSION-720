using System.Text.Json;
using TFusion.Foundation.Diagnostics;
using TFusion.Foundation.Results;

namespace TFusion.Foundation.Lifecycle;

public interface IAtomicTextWriter
{
    Result Write(string destinationPath, string content);
}

public sealed record StartupObservation(
    bool PreviousSessionWasUnclean,
    bool PreviousMarkerWasInvalid,
    Guid SessionId);

public sealed class StartupSentinel
{
    private const int CurrentSchemaVersion = 1;
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private readonly string markerPath;
    private readonly IAtomicTextWriter writer;

    public StartupSentinel(string markerPath, IAtomicTextWriter? writer = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(markerPath);
        this.markerPath = Path.GetFullPath(markerPath);
        this.writer = writer ?? new PhysicalAtomicTextWriter();
    }

    public Result<StartupObservation> BeginSession(Guid sessionId, DateTimeOffset utcNow)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("A session identifier cannot be empty.", nameof(sessionId));
        }

        var previousWasUnclean = false;
        var previousWasInvalid = false;
        var warnings = new List<CadDiagnostic>();

        if (File.Exists(markerPath))
        {
            try
            {
                var previous = JsonSerializer.Deserialize<SentinelState>(
                    File.ReadAllText(markerPath),
                    SerializerOptions);

                if (previous is null || previous.SchemaVersion != CurrentSchemaVersion)
                {
                    previousWasInvalid = true;
                }
                else
                {
                    previousWasUnclean = !previous.CleanShutdown;
                }
            }
            catch (Exception exception) when (
                exception is IOException
                    or UnauthorizedAccessException
                    or JsonException
                    or NotSupportedException)
            {
                previousWasInvalid = true;
                warnings.Add(new CadDiagnostic(
                    FoundationDiagnosticCodes.StartupStateInvalid,
                    DiagnosticSeverity.Warning,
                    "The previous startup marker was invalid and has been reset.",
                    exception.Message));
            }
        }

        if (previousWasInvalid && warnings.Count == 0)
        {
            warnings.Add(new CadDiagnostic(
                FoundationDiagnosticCodes.StartupStateInvalid,
                DiagnosticSeverity.Warning,
                "The previous startup marker used an unsupported schema and has been reset."));
        }

        var state = new SentinelState(
            CurrentSchemaVersion,
            sessionId,
            utcNow.ToUniversalTime(),
            CleanShutdown: false,
            utcNow.ToUniversalTime());

        var writeResult = writer.Write(markerPath, JsonSerializer.Serialize(state, SerializerOptions));
        if (writeResult.IsFailure)
        {
            return Result<StartupObservation>.Failure(writeResult.Diagnostics);
        }

        return Result<StartupObservation>.Success(
            new StartupObservation(previousWasUnclean, previousWasInvalid, sessionId),
            warnings);
    }

    public Result MarkClean(Guid sessionId, DateTimeOffset utcNow)
    {
        if (sessionId == Guid.Empty)
        {
            throw new ArgumentException("A session identifier cannot be empty.", nameof(sessionId));
        }

        SentinelState? current;

        try
        {
            current = JsonSerializer.Deserialize<SentinelState>(
                File.ReadAllText(markerPath),
                SerializerOptions);
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or JsonException
                or NotSupportedException)
        {
            return Result.Failure(new CadDiagnostic(
                FoundationDiagnosticCodes.StartupStateInvalid,
                DiagnosticSeverity.Error,
                "The current startup marker could not be read.",
                exception.Message));
        }

        if (current is null
            || current.SchemaVersion != CurrentSchemaVersion
            || current.SessionId != sessionId)
        {
            return Result.Failure(new CadDiagnostic(
                FoundationDiagnosticCodes.StartupSessionMismatch,
                DiagnosticSeverity.Error,
                "The current startup session could not be verified."));
        }

        var cleanState = current with
        {
            CleanShutdown = true,
            UpdatedUtc = utcNow.ToUniversalTime(),
        };

        return writer.Write(markerPath, JsonSerializer.Serialize(cleanState, SerializerOptions));
    }

    private sealed record SentinelState(
        int SchemaVersion,
        Guid SessionId,
        DateTimeOffset StartedUtc,
        bool CleanShutdown,
        DateTimeOffset UpdatedUtc);
}

public sealed class PhysicalAtomicTextWriter : IAtomicTextWriter
{
    public Result Write(string destinationPath, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destinationPath);
        ArgumentNullException.ThrowIfNull(content);

        var fullDestination = Path.GetFullPath(destinationPath);
        var directory = Path.GetDirectoryName(fullDestination);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return Failure("The destination has no parent directory.");
        }

        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(fullDestination)}.{Guid.NewGuid():N}.tmp");

        try
        {
            Directory.CreateDirectory(directory);

            using (var stream = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                FileOptions.WriteThrough))
            using (var textWriter = new StreamWriter(stream))
            {
                textWriter.Write(content);
                textWriter.Flush();
                stream.Flush(flushToDisk: true);
            }

            File.Move(temporaryPath, fullDestination, overwrite: true);
            return Result.Success();
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or NotSupportedException)
        {
            var cleanupFailure = TryDelete(temporaryPath);
            var technicalDetail = cleanupFailure is null
                ? exception.Message
                : $"{exception.Message} Temporary-marker cleanup also failed: {cleanupFailure}";
            return Failure(technicalDetail);
        }
    }

    private static Result Failure(string technicalDetail) =>
        Result.Failure(new CadDiagnostic(
            FoundationDiagnosticCodes.StartupStateWriteFailed,
            DiagnosticSeverity.Error,
            "The startup state could not be written safely.",
            technicalDetail));

    private static string? TryDelete(string path)
    {
        try
        {
            File.Delete(path);
            return null;
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException)
        {
            return exception.Message;
        }
    }
}
