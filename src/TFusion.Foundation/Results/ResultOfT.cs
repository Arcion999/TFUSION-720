using TFusion.Foundation.Diagnostics;

namespace TFusion.Foundation.Results;

public sealed class Result<T>
    where T : notnull
{
    private readonly T? value;

    internal Result(bool isSuccess, T? value, IEnumerable<CadDiagnostic> diagnostics)
    {
        var diagnosticArray = diagnostics.ToArray();

        if (diagnosticArray.Any(diagnostic => diagnostic is null))
        {
            throw new ArgumentException("Diagnostics cannot contain null entries.", nameof(diagnostics));
        }

        var containsError = diagnosticArray.Any(diagnostic => diagnostic.IsError);

        if (isSuccess && containsError)
        {
            throw new ArgumentException("A successful result cannot contain error diagnostics.", nameof(diagnostics));
        }

        if (!isSuccess && !containsError)
        {
            throw new ArgumentException("A failed result must contain at least one error diagnostic.", nameof(diagnostics));
        }

        if (isSuccess)
        {
            ArgumentNullException.ThrowIfNull(value);
        }

        IsSuccess = isSuccess;
        this.value = value;
        Diagnostics = Array.AsReadOnly(diagnosticArray);
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public IReadOnlyList<CadDiagnostic> Diagnostics { get; }

    public T Value =>
        IsSuccess
            ? value!
            : throw new InvalidOperationException("A failed result does not contain a value.");

}
