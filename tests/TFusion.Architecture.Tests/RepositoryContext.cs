using System.Diagnostics;

namespace TFusion.Architecture.Tests;

internal static class RepositoryContext
{
    public static string Root { get; } = FindRoot();

    public static IEnumerable<string> Files(string searchPattern, SearchOption searchOption) =>
        Directory.EnumerateFiles(Root, searchPattern, searchOption)
            .Where(path => !IsIgnoredBuildPath(path));

    public static string Read(string relativePath) => File.ReadAllText(Path.Combine(Root, relativePath));

    public static IReadOnlyList<string> TrackedFiles()
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "ls-files -z",
                WorkingDirectory = Root,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            },
        };

        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        Assert.True(process.ExitCode == 0, error);
        return output.Split('\0', StringSplitOptions.RemoveEmptyEntries);
    }

    private static string FindRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "TFUSION-720.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Could not locate the TFUSION-720 repository root.");
    }

    private static bool IsIgnoredBuildPath(string path)
    {
        var relative = Path.GetRelativePath(Root, path);
        var segments = relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return segments.Any(segment => segment is "bin" or "obj" or "artifacts" or ".git");
    }
}
