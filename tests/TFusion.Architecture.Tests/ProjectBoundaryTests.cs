using System.Xml.Linq;

namespace TFusion.Architecture.Tests;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void M1_A01_SolutionContainsExactlyFiveMilestoneProjects()
    {
        var projects = RepositoryContext.Files("*.csproj", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(RepositoryContext.Root, path).Replace('\\', '/'))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
        [
            "src/TFusion.App/TFusion.App.csproj",
            "src/TFusion.Diagnostics/TFusion.Diagnostics.csproj",
            "src/TFusion.Foundation/TFusion.Foundation.csproj",
            "tests/TFusion.Architecture.Tests/TFusion.Architecture.Tests.csproj",
            "tests/TFusion.Foundation.Tests/TFusion.Foundation.Tests.csproj",
        ], projects);

        var solution = RepositoryContext.Read("TFUSION-720.sln");
        Assert.Equal(5, solution.Split('\n').Count(line => line.StartsWith("Project(\"", StringComparison.Ordinal)));
    }

    [Fact]
    public void M1_A02_FoundationReferencesNoOtherTFusionAssembly()
    {
        var project = LoadProject("src/TFusion.Foundation/TFusion.Foundation.csproj");
        Assert.Empty(project.Descendants("ProjectReference"));
        Assert.Empty(project.Descendants("PackageReference"));
    }

    [Fact]
    public void M1_A03_FoundationHasNoForbiddenFrameworkOrPackageReference()
    {
        var combined = string.Join('\n', Directory.EnumerateFiles(
                Path.Combine(RepositoryContext.Root, "src", "TFusion.Foundation"),
                "*",
                SearchOption.AllDirectories)
            .Where(path => Path.GetExtension(path) is ".cs" or ".csproj")
            .Select(File.ReadAllText));
        var forbidden = new[]
        {
            "PresentationFramework",
            "PresentationCore",
            "WindowsBase",
            "System.Windows.Forms",
            "Vortice",
            "OpenCascade",
            "System.Net",
        };

        Assert.All(forbidden, value => Assert.DoesNotContain(value, combined, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void M1_A04_ExecutableProjectsReferenceFoundationWithoutReverseReference()
    {
        AssertReferencesFoundation("src/TFusion.App/TFusion.App.csproj");
        AssertReferencesFoundation("src/TFusion.Diagnostics/TFusion.Diagnostics.csproj");
        Assert.Empty(LoadProject("src/TFusion.Foundation/TFusion.Foundation.csproj").Descendants("ProjectReference"));
    }

    [Fact]
    public void M1_A05_NoNativeInteropDeclarationExists()
    {
        var sources = ReadAllCSharp();
        Assert.DoesNotContain("[" + "DllImport(", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("[" + "LibraryImport(", sources, StringComparison.Ordinal);
    }

    [Fact]
    public void M1_A06_NoProjectEnablesUnsafeCode()
    {
        foreach (var projectPath in RepositoryContext.Files("*.csproj", SearchOption.AllDirectories))
        {
            var project = XDocument.Load(projectPath);
            Assert.DoesNotContain(project.Descendants("AllowUnsafeBlocks"), element =>
                string.Equals(element.Value, "true", StringComparison.OrdinalIgnoreCase));
        }

        var sources = ReadAllCSharp();
        Assert.DoesNotContain("unsafe" + " {", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("unsafe" + " class", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("unsafe" + " struct", sources, StringComparison.Ordinal);
        Assert.DoesNotContain("unsafe" + " void", sources, StringComparison.Ordinal);
    }

    private static XDocument LoadProject(string relativePath) =>
        XDocument.Load(Path.Combine(RepositoryContext.Root, relativePath));

    private static string ReadAllCSharp() => string.Join('\n',
        RepositoryContext.Files("*.cs", SearchOption.AllDirectories).Select(File.ReadAllText));

    private static void AssertReferencesFoundation(string relativePath)
    {
        var references = LoadProject(relativePath)
            .Descendants("ProjectReference")
            .Select(element => element.Attribute("Include")?.Value.Replace('\\', '/'));
        Assert.Contains(references, value => value?.EndsWith(
            "TFusion.Foundation/TFusion.Foundation.csproj",
            StringComparison.Ordinal) == true);
    }
}
