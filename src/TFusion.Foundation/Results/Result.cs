using TFusion.Foundation.Diagnostics;

namespace TFusion.Foundation.Results;

public sealed class Result
{
    private Result(bool isSuccess, IEnumerable<CadDiagnostic> diagnostics)
    {
        var diagnosticArray = diagnostics.ToArray();
        Validate(isSuccess, diagnosticArray);

        IsSuccess = isSuccess;
        Diagnostics = Array.AsReadOnly(diagnosticArray);
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<CadDiagnostic> Diagnostics { get; }

    public static Result Success(IEnumerable<CadDiagnostic>? diagnostics = null) =>
        new(true, diagnostics ?? []);

    public static Result Failure(params CadDiagnostic[] diagnostics) =>
        new(false, diagnostics);

    public static Result Failure(IEnumerable<CadDiagnostic> diagnostics) =>
        new(false, diagnostics);

    private static void Validate(bool isSuccess, IReadOnlyCollection<CadDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        if (diagnostics.Any(diagnostic => diagnostic is null))
        {
            throw new ArgumentException("Diagnostics cannot contain null entries.", nameof(diagnostics));
        }

        var containsError = diagnostics.Any(diagnostic => diagnostic.IsError);

        if (isSuccess && containsError)
        {
            throw new ArgumentException("A successful result cannot contain error diagnostics.", nameof(diagnostics));
        }

        if (!isSuccess && !containsError)
        {
            throw new ArgumentException("A failed result must contain at least one error diagnostic.", nameof(diagnostics));
        }
    }
}
