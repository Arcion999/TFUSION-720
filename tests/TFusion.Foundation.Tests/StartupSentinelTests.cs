using System.Text.Json;
using TFusion.Foundation.Diagnostics;
using TFusion.Foundation.Lifecycle;
using TFusion.Foundation.Results;

namespace TFusion.Foundation.Tests;

public sealed class StartupSentinelTests : IDisposable
{
    private readonly string testRoot = Path.Combine(
        Path.GetTempPath(),
        "TFUSION-720-tests",
        Guid.NewGuid().ToString("N"));

    private string MarkerPath => Path.Combine(testRoot, "Recovery", "startup-state.json");

    [Fact]
    public void M1_U07_CleanStartAndStopRoundTripIsDetectedAsClean()
    {
        var firstSession = Guid.NewGuid();
        var sentinel = new StartupSentinel(MarkerPath);

        var firstStart = sentinel.BeginSession(firstSession, DateTimeOffset.UtcNow);
        Assert.True(firstStart.IsSuccess);
        Assert.False(firstStart.Value.PreviousSessionWasUnclean);
        Assert.True(sentinel.MarkClean(firstSession, DateTimeOffset.UtcNow).IsSuccess);

        var secondStart = sentinel.BeginSession(Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.True(secondStart.IsSuccess);
        Assert.False(secondStart.Value.PreviousSessionWasUnclean);
    }

    [Fact]
    public void M1_U08_PreExistingUncleanMarkerIsReported()
    {
        var sentinel = new StartupSentinel(MarkerPath);
        Assert.True(sentinel.BeginSession(Guid.NewGuid(), DateTimeOffset.UtcNow).IsSuccess);

        var nextStart = sentinel.BeginSession(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.True(nextStart.IsSuccess);
        Assert.True(nextStart.Value.PreviousSessionWasUnclean);
    }

    [Fact]
    public void M1_U09_CorruptMarkerProducesDiagnosticAndSafeReset()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(MarkerPath)!);
        File.WriteAllText(MarkerPath, "{not-json");
        var sessionId = Guid.NewGuid();

        var result = new StartupSentinel(MarkerPath).BeginSession(sessionId, DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.PreviousMarkerWasInvalid);
        Assert.Contains(result.Diagnostics, diagnostic =>
            diagnostic.Code == FoundationDiagnosticCodes.StartupStateInvalid);
        using var parsed = JsonDocument.Parse(File.ReadAllText(MarkerPath));
        Assert.Equal(sessionId, parsed.RootElement.GetProperty("sessionId").GetGuid());
    }

    [Fact]
    public void M1_U10_InjectedWriterFailurePreservesCompletePreviousMarker()
    {
        var sentinel = new StartupSentinel(MarkerPath);
        Assert.True(sentinel.BeginSession(Guid.NewGuid(), DateTimeOffset.UtcNow).IsSuccess);
        var previousContent = File.ReadAllText(MarkerPath);
        var faultingSentinel = new StartupSentinel(MarkerPath, new RejectingWriter());

        var result = faultingSentinel.BeginSession(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(previousContent, File.ReadAllText(MarkerPath));
        using var parsed = JsonDocument.Parse(File.ReadAllText(MarkerPath));
        Assert.Equal(1, parsed.RootElement.GetProperty("schemaVersion").GetInt32());
    }

    [Fact]
    public void MarkCleanRejectsMissingOrMismatchedSession()
    {
        var sentinel = new StartupSentinel(MarkerPath);
        Assert.True(sentinel.MarkClean(Guid.NewGuid(), DateTimeOffset.UtcNow).IsFailure);
        Assert.True(sentinel.BeginSession(Guid.NewGuid(), DateTimeOffset.UtcNow).IsSuccess);
        var mismatch = sentinel.MarkClean(Guid.NewGuid(), DateTimeOffset.UtcNow);
        Assert.True(mismatch.IsFailure);
        Assert.Equal(FoundationDiagnosticCodes.StartupSessionMismatch, mismatch.Diagnostics[0].Code);
    }

    [Fact]
    public void UnsupportedMarkerSchemaProducesWarningAndIsReset()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(MarkerPath)!);
        File.WriteAllText(MarkerPath, "{\"schemaVersion\":999}");

        var result = new StartupSentinel(MarkerPath).BeginSession(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.PreviousMarkerWasInvalid);
        Assert.Single(result.Diagnostics);
    }

    [Fact]
    public void InvalidSessionArgumentsAreRejected()
    {
        var sentinel = new StartupSentinel(MarkerPath);

        Assert.Throws<ArgumentException>(() => sentinel.BeginSession(Guid.Empty, DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => sentinel.MarkClean(Guid.Empty, DateTimeOffset.UtcNow));
        Assert.Throws<ArgumentException>(() => new StartupSentinel(" "));
    }

    [Fact]
    public void CorruptCurrentMarkerMakesMarkCleanFailSafely()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(MarkerPath)!);
        File.WriteAllText(MarkerPath, "invalid-json");

        var result = new StartupSentinel(MarkerPath).MarkClean(Guid.NewGuid(), DateTimeOffset.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(FoundationDiagnosticCodes.StartupStateInvalid, result.Diagnostics[0].Code);
    }

    [Fact]
    public void PhysicalWriterAtomicallyCreatesAndReplacesContent()
    {
        var writer = new PhysicalAtomicTextWriter();

        Assert.True(writer.Write(MarkerPath, "first").IsSuccess);
        Assert.Equal("first", File.ReadAllText(MarkerPath));
        Assert.True(writer.Write(MarkerPath, "second").IsSuccess);
        Assert.Equal("second", File.ReadAllText(MarkerPath));
        Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(MarkerPath)!, "*.tmp"));
    }

    [Fact]
    public void PhysicalWriterReturnsDiagnosticWhenDestinationIsDirectory()
    {
        Directory.CreateDirectory(MarkerPath);

        var result = new PhysicalAtomicTextWriter().Write(MarkerPath, "content");

        Assert.True(result.IsFailure);
        Assert.Equal(FoundationDiagnosticCodes.StartupStateWriteFailed, result.Diagnostics[0].Code);
    }

    public void Dispose()
    {
        if (Directory.Exists(testRoot))
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }

    private sealed class RejectingWriter : IAtomicTextWriter
    {
        public Result Write(string destinationPath, string content) => Result.Failure(new CadDiagnostic(
            FoundationDiagnosticCodes.StartupStateWriteFailed,
            DiagnosticSeverity.Error,
            "Injected write failure."));
    }
}
