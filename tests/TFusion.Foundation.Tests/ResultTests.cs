using TFusion.Foundation.Diagnostics;
using TFusion.Foundation.Results;

namespace TFusion.Foundation.Tests;

public sealed class ResultTests
{
    private static readonly CadDiagnostic Warning = new(
        FoundationDiagnosticCodes.InvalidConfiguration,
        DiagnosticSeverity.Warning,
        "Warning");

    private static readonly CadDiagnostic Error = new(
        FoundationDiagnosticCodes.InvalidConfiguration,
        DiagnosticSeverity.Error,
        "Error");

    [Fact]
    public void M1_U01_ResultFactoriesEnforceSuccessAndFailureInvariants()
    {
        var success = Result.Success([Warning]);
        var failure = Result.Failure(Error);

        Assert.True(success.IsSuccess);
        Assert.False(success.IsFailure);
        Assert.True(failure.IsFailure);
        Assert.Throws<ArgumentException>(() => Result.Success([Error]));
        Assert.Throws<ArgumentException>(() => Result.Failure(Warning));
        Assert.Throws<ArgumentException>(() => Result.Failure([]));
        Assert.Throws<ArgumentException>(() => Result<string>.Success("value", [Error]));
        Assert.Throws<ArgumentException>(() => Result<string>.Failure(Warning));
        Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
    }

    [Fact]
    public void M1_U02_FailedGenericResultCannotExposeValue()
    {
        var result = Result<string>.Failure(Error);

        Assert.True(result.IsFailure);
        var exception = Assert.Throws<InvalidOperationException>(() => result.Value);
        Assert.Contains("does not contain a value", exception.Message, StringComparison.Ordinal);
    }
}
