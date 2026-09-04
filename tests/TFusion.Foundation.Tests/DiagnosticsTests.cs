using TFusion.Foundation.Diagnostics;

namespace TFusion.Foundation.Tests;

public sealed class DiagnosticsTests
{
    [Fact]
    public void M1U03DiagnosticCopiesContextAndCausalOrder()
    {
        var context = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["operation"] = "startup",
        };
        var firstCause = new CadDiagnostic(
            FoundationDiagnosticCodes.StorageUnavailable,
            DiagnosticSeverity.Error,
            "Storage unavailable",
            "technical detail");
        var secondCause = new CadDiagnostic(
            FoundationDiagnosticCodes.InvalidConfiguration,
            DiagnosticSeverity.Warning,
            "Configuration warning");
        var causes = new[] { firstCause, secondCause };

        var diagnostic = new CadDiagnostic(
            FoundationDiagnosticCodes.StartupStateWriteFailed,
            DiagnosticSeverity.Fatal,
            "Startup state could not be written",
            "disk rejected write",
            context,
            causes);

        context["operation"] = "mutated";
        causes[0] = secondCause;

        Assert.Equal("TFN-FND-0004", diagnostic.Code.Value);
        Assert.Equal(DiagnosticSeverity.Fatal, diagnostic.Severity);
        Assert.True(diagnostic.IsError);
        Assert.Equal("Startup state could not be written", diagnostic.UserMessage);
        Assert.Equal("disk rejected write", diagnostic.TechnicalDetail);
        Assert.Equal("startup", diagnostic.Context["operation"]);
        Assert.Same(firstCause, diagnostic.Causes[0]);
        Assert.Same(secondCause, diagnostic.Causes[1]);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public void M1U12LoggingPolicyRejectsUnboundedValues(long bytes, int files)
    {
        Assert.ThrowsAny<ArgumentOutOfRangeException>(() => new LoggingPolicy(bytes, files));
    }

    [Fact]
    public void M1U12DefaultLoggingPolicyIsExplicitlyBounded()
    {
        Assert.Equal(10 * 1024 * 1024, LoggingPolicy.Default.FileSizeLimitBytes);
        Assert.Equal(14, LoggingPolicy.Default.RetainedFileCountLimit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("lower-case")]
    [InlineData("HAS SPACE")]
    [InlineData("SLASH/CODE")]
    public void DiagnosticCodeRejectsNonMachineSafeValues(string value)
    {
        Assert.Throws<ArgumentException>(() => new DiagnosticCode(value));
    }

    [Fact]
    public void NonErrorDiagnosticUsesEmptyImmutableCollections()
    {
        var diagnostic = new CadDiagnostic(
            FoundationDiagnosticCodes.UnsupportedPlatform,
            DiagnosticSeverity.Information,
            "Information");

        Assert.False(diagnostic.IsError);
        Assert.Empty(diagnostic.Context);
        Assert.Empty(diagnostic.Causes);
        Assert.Null(diagnostic.TechnicalDetail);
        Assert.Equal("TFN-FND-0006", diagnostic.Code.ToString());
    }

    [Fact]
    public void DiagnosticCodeAcceptsItsEntireDeclaredAlphabet()
    {
        var code = new DiagnosticCode("ABC-123_DEF.VALUE");
        Assert.Equal("ABC-123_DEF.VALUE", code.Value);
    }
}
