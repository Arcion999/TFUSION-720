using System.Xml.Linq;

namespace TFusion.Architecture.Tests;

public sealed class ProjectBoundaryTests
{
    [Fact]
    public void M2A01SolutionContainsTheApprovedManagedProjects()
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
            "src/TFusion.Kernel.Interop/TFusion.Kernel.Interop.csproj",
            "tests/TFusion.Architecture.Tests/TFusion.Architecture.Tests.csproj",
            "tests/TFusion.Foundation.Tests/TFusion.Foundation.Tests.csproj",
            "tests/TFusion.Kernel.Tests/TFusion.Kernel.Tests.csproj",
        ], projects);

        var solution = RepositoryContext.Read("TFUSION-720.sln");
        Assert.Equal(7, solution.Split('\n').Count(line => line.StartsWith("Project(\"", StringComparison.Ordinal)));
    }

    [Fact]
    public void M1A02FoundationReferencesNoOtherTFusionAssembly()
    {
        var project = LoadProject("src/TFusion.Foundation/TFusion.Foundation.csproj");
        Assert.Empty(project.Descendants("ProjectReference"));
        Assert.Empty(project.Descendants("PackageReference"));
    }

    [Fact]
    public void M1A03FoundationHasNoForbiddenFrameworkOrPackageReference()
    {
        var foundationRoot = Path.Combine(RepositoryContext.Root, "src", "TFusion.Foundation");
        var combined = string.Join('\n', RepositoryContext.Files("*", SearchOption.AllDirectories)
            .Where(path => path.StartsWith(foundationRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal)
                && Path.GetExtension(path) is ".cs" or ".csproj")
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
    public void M1A04ExecutableProjectsReferenceFoundationWithoutReverseReference()
    {
        AssertReferencesFoundation("src/TFusion.App/TFusion.App.csproj");
        AssertReferencesFoundation("src/TFusion.Diagnostics/TFusion.Diagnostics.csproj");
        AssertReferencesFoundation("src/TFusion.Kernel.Interop/TFusion.Kernel.Interop.csproj");
        Assert.Empty(LoadProject("src/TFusion.Foundation/TFusion.Foundation.csproj").Descendants("ProjectReference"));
    }

    [Fact]
    public void M2A02NativeImportsExistOnlyInTheDedicatedInteropAssembly()
    {
        var interopRoot = Path.Combine(RepositoryContext.Root, "src", "TFusion.Kernel.Interop");
        var filesWithImports = RepositoryContext.Files("*.cs", SearchOption.AllDirectories)
            .Where(path => File.ReadAllText(path).Contains("[" + "DllImport(", StringComparison.Ordinal))
            .ToArray();

        Assert.NotEmpty(filesWithImports);
        Assert.All(filesWithImports, path => Assert.StartsWith(
            interopRoot + Path.DirectorySeparatorChar,
            path,
            StringComparison.Ordinal));
        Assert.DoesNotContain("[" + "LibraryImport(", ReadAllCSharp(), StringComparison.Ordinal);

        var nativeMethods = RepositoryContext.Read("src/TFusion.Kernel.Interop/Native/NativeMethods.cs");
        Assert.Contains("CallingConvention = CallingConvention.Cdecl", nativeMethods, StringComparison.Ordinal);
        Assert.Contains("ExactSpelling = true", nativeMethods, StringComparison.Ordinal);
    }

    [Fact]
    public void M2A03ManagedCodeDoesNotExposeOcctCppTypes()
    {
        var sourceRoot = Path.Combine(RepositoryContext.Root, "src");
        var sources = string.Join('\n', RepositoryContext.Files("*.cs", SearchOption.AllDirectories)
            .Where(path => path.StartsWith(sourceRoot + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .Select(File.ReadAllText));
        var forbidden = new[] { "TopoDS_", "Handle(Geom", "Standard_Transient", "opencascade::" };
        Assert.All(forbidden, value => Assert.DoesNotContain(value, sources, StringComparison.Ordinal));
    }

    [Fact]
    public void M1A06NoProjectEnablesUnsafeCode()
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
