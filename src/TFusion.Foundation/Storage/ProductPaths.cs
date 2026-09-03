using TFusion.Foundation.Diagnostics;
using TFusion.Foundation.Results;

namespace TFusion.Foundation.Storage;

public sealed class ProductPaths
{
    private const string ProductDirectoryName = "TFUSION-720";

    private ProductPaths(string root)
    {
        Root = root;
        Logs = Child(root, "Logs");
        Recovery = Child(root, "Recovery");
        CrashDumps = Child(root, "CrashDumps");
        Settings = Child(root, "Settings");
        Temp = Child(root, "Temp");
    }

    public string Root { get; }
    public string Logs { get; }
    public string Recovery { get; }
    public string CrashDumps { get; }
    public string Settings { get; }
    public string Temp { get; }

    public string StartupSentinelFile => Path.Combine(Recovery, "startup-state.json");

    public static Result<ProductPaths> CreateDefault()
    {
        var localApplicationData =
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        if (string.IsNullOrWhiteSpace(localApplicationData))
        {
            return Result<ProductPaths>.Failure(new CadDiagnostic(
                FoundationDiagnosticCodes.StorageUnavailable,
                DiagnosticSeverity.Error,
                "Local application storage is unavailable.",
                "Environment.SpecialFolder.LocalApplicationData resolved to an empty path."));
        }

        return FromRoot(Path.Combine(localApplicationData, ProductDirectoryName));
    }

    public static Result<ProductPaths> FromRoot(string root)
    {
        if (string.IsNullOrWhiteSpace(root))
        {
            return Result<ProductPaths>.Failure(new CadDiagnostic(
                FoundationDiagnosticCodes.InvalidConfiguration,
                DiagnosticSeverity.Error,
                "The application storage path is invalid.",
                "The supplied root path was null, empty, or whitespace."));
        }

        try
        {
            return Result<ProductPaths>.Success(new ProductPaths(Path.GetFullPath(root)));
        }
        catch (Exception exception) when (
            exception is ArgumentException
                or NotSupportedException
                or PathTooLongException)
        {
            return Result<ProductPaths>.Failure(new CadDiagnostic(
                FoundationDiagnosticCodes.InvalidConfiguration,
                DiagnosticSeverity.Error,
                "The application storage path is invalid.",
                exception.Message));
        }
    }

    public Result EnsureCreated()
    {
        try
        {
            foreach (var directory in EnumerateDirectories())
            {
                Directory.CreateDirectory(directory);
            }

            return Result.Success();
        }
        catch (Exception exception) when (
            exception is IOException
                or UnauthorizedAccessException
                or NotSupportedException)
        {
            return Result.Failure(new CadDiagnostic(
                FoundationDiagnosticCodes.StorageUnavailable,
                DiagnosticSeverity.Error,
                "TFUSION-720 could not initialize its local application storage.",
                exception.Message,
                [new KeyValuePair<string, string>("Root", Root)]));
        }
    }

    public IReadOnlyList<string> EnumerateDirectories() =>
        Array.AsReadOnly([Root, Logs, Recovery, CrashDumps, Settings, Temp]);

    public bool Contains(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var candidate = Path.GetFullPath(path);
        var rootWithSeparator = Root.EndsWith(Path.DirectorySeparatorChar)
            ? Root
            : Root + Path.DirectorySeparatorChar;

        return candidate.Equals(Root, PathComparison)
            || candidate.StartsWith(rootWithSeparator, PathComparison);
    }

    private static StringComparison PathComparison =>
        OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    private static string Child(string root, string name)
    {
        var child = Path.GetFullPath(Path.Combine(root, name));
        var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar)
            ? root
            : root + Path.DirectorySeparatorChar;

        if (!child.StartsWith(rootWithSeparator, PathComparison))
        {
            throw new InvalidOperationException("A product path escaped the configured application root.");
        }

        return child;
    }
}
