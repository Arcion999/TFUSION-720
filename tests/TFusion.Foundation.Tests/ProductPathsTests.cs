using TFusion.Foundation.Storage;

namespace TFusion.Foundation.Tests;

public sealed class ProductPathsTests : IDisposable
{
    private readonly string testRoot = Path.Combine(
        Path.GetTempPath(),
        "TFUSION-720-tests",
        Guid.NewGuid().ToString("N"));

    [Fact]
    public void M1U05OnlyCreatesExpectedDescendantsOfInjectedRoot()
    {
        var result = ProductPaths.FromRoot(testRoot);
        Assert.True(result.IsSuccess);
        var paths = result.Value;

        Assert.All(paths.EnumerateDirectories(), path => Assert.True(paths.Contains(path)));
        Assert.True(paths.Contains(paths.StartupSentinelFile));
        Assert.False(paths.Contains(Path.GetDirectoryName(testRoot)!));
        Assert.True(paths.EnsureCreated().IsSuccess);
        Assert.All(paths.EnumerateDirectories(), path => Assert.True(Directory.Exists(path)));
    }

    [Fact]
    public void M1U06InvalidPathFailsWithoutCurrentDirectoryFallback()
    {
        var originalCurrentDirectory = Environment.CurrentDirectory;
        var result = ProductPaths.FromRoot(" ");

        Assert.True(result.IsFailure);
        Assert.Equal(originalCurrentDirectory, Environment.CurrentDirectory);
        Assert.Equal("TFN-FND-0001", result.Diagnostics[0].Code.Value);
    }

    [Fact]
    public void M1U06PathUnderExistingFileReturnsControlledFailure()
    {
        Directory.CreateDirectory(testRoot);
        var blockingFile = Path.Combine(testRoot, "blocking-file");
        File.WriteAllText(blockingFile, "not a directory");

        var pathsResult = ProductPaths.FromRoot(Path.Combine(blockingFile, "root"));
        Assert.True(pathsResult.IsSuccess);
        var createResult = pathsResult.Value.EnsureCreated();

        Assert.True(createResult.IsFailure);
        Assert.Equal("TFN-FND-0002", createResult.Diagnostics[0].Code.Value);
        Assert.False(Directory.Exists(Path.Combine(Environment.CurrentDirectory, "TFUSION-720")));
    }

    [Fact]
    public void DefaultPathsResolveBelowLocalApplicationData()
    {
        var result = ProductPaths.CreateDefault();

        Assert.True(result.IsSuccess);
        Assert.EndsWith("TFUSION-720", result.Value.Root, StringComparison.Ordinal);
        Assert.All(result.Value.EnumerateDirectories(), path => Assert.True(result.Value.Contains(path)));
    }

    public void Dispose()
    {
        if (Directory.Exists(testRoot))
        {
            Directory.Delete(testRoot, recursive: true);
        }
    }
}
