using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace TFusion.Architecture.Tests;

public sealed partial class RepositoryPolicyTests
{
    [Fact]
    public void M1A07PackageVersionsAreCentralExactLockedAndStable()
    {
        var central = XDocument.Load(Path.Combine(RepositoryContext.Root, "Directory.Packages.props"));
        var versions = central.Descendants("PackageVersion").ToArray();
        Assert.NotEmpty(versions);
        Assert.All(versions, element =>
        {
            var value = element.Attribute("Version")?.Value;
            Assert.NotNull(value);
            Assert.Matches(ExactVersionRegex(), value);
            Assert.DoesNotContain('-', value);
        });

        foreach (var projectPath in RepositoryContext.Files("*.csproj", SearchOption.AllDirectories))
        {
            var project = XDocument.Load(projectPath);
            Assert.All(project.Descendants("PackageReference"), reference =>
            {
                Assert.Null(reference.Attribute("Version"));
                Assert.Null(reference.Element("Version"));
            });

            if (project.Descendants("PackageReference").Any())
            {
                Assert.True(
                    File.Exists(Path.Combine(Path.GetDirectoryName(projectPath)!, "packages.lock.json")),
                    $"Missing package lock for {projectPath}");
            }
        }
    }

    [Fact]
    public void M1A08AuthoritativeFilesMatchRecordedSha256Hashes()
    {
        var specificationDirectory = Path.Combine(RepositoryContext.Root, "docs", "specification");
        var manifestLines = File.ReadAllLines(Path.Combine(specificationDirectory, "SOURCES.sha256"));
        Assert.Equal(3, manifestLines.Length);

        foreach (var line in manifestLines)
        {
            var pieces = line.Split("  ", 2, StringSplitOptions.None);
            Assert.Equal(2, pieces.Length);
            var bytes = File.ReadAllBytes(Path.Combine(specificationDirectory, pieces[1]));
            var actual = Convert.ToHexStringLower(SHA256.HashData(bytes));
            Assert.Equal(pieces[0], actual);
        }
    }

    [Fact]
    public void M1A09NoGeneratedOrUserLocalFileIsTracked()
    {
        var forbiddenSegments = new[] { "bin/", "obj/", "artifacts/", "TestResults/", ".vs/" };
        var forbiddenExtensions = new[] { ".user", ".suo", ".dmp", ".log" };

        foreach (var relativePath in RepositoryContext.TrackedFiles())
        {
            var normalized = relativePath.Replace('\\', '/');
            Assert.DoesNotContain(forbiddenSegments, segment =>
                normalized.Contains(segment, StringComparison.OrdinalIgnoreCase));
            Assert.DoesNotContain(forbiddenExtensions, extension =>
                normalized.EndsWith(extension, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void M1A10WorkflowActionsUseImmutableCommitPins()
    {
        foreach (var workflowPath in RepositoryContext.Files("*.yml", SearchOption.AllDirectories)
                     .Where(path => path.Contains(
                         $"{Path.DirectorySeparatorChar}.github{Path.DirectorySeparatorChar}workflows{Path.DirectorySeparatorChar}",
                         StringComparison.Ordinal)))
        {
            var lines = File.ReadAllLines(workflowPath);
            var actionLines = lines.Where(line => line.TrimStart().StartsWith("uses:", StringComparison.Ordinal));
            Assert.NotEmpty(actionLines);
            Assert.All(actionLines, line => Assert.Matches(ActionPinRegex(), line));
        }
    }

    [Fact]
    public void M2A04ReadmeMakesNoUserFacingCadCapabilityClaim()
    {
        var readme = RepositoryContext.Read("README.md");
        Assert.Contains("contains no user-facing CAD functionality", readme, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("no file-format support", readme, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("supports STEP", readme, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("working CAD", readme, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void M1A12NoticesCoverEveryLockedDependency()
    {
        var notices = RepositoryContext.Read("THIRD_PARTY_NOTICES.md");
        var dependencyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var lockPath in RepositoryContext.Files("packages.lock.json", SearchOption.AllDirectories))
        {
            using var document = JsonDocument.Parse(File.ReadAllText(lockPath));
            foreach (var target in document.RootElement.GetProperty("dependencies").EnumerateObject())
            {
                foreach (var dependency in target.Value.EnumerateObject())
                {
                    var type = dependency.Value.GetProperty("type").GetString();
                    if (!string.Equals(type, "Project", StringComparison.Ordinal))
                    {
                        dependencyNames.Add(dependency.Name);
                    }
                }
            }
        }

        Assert.NotEmpty(dependencyNames);
        Assert.All(dependencyNames, package => Assert.Contains(package, notices, StringComparison.OrdinalIgnoreCase));
        Assert.Contains("Open CASCADE Technology", notices, StringComparison.Ordinal);
        Assert.Contains("LGPL-2.1-only WITH OCCT-exception-1.0", notices, StringComparison.Ordinal);
    }

    [Fact]
    public void M2A05OcctDependencyIsManifestPinnedToTheApprovedBaseline()
    {
        using var manifest = JsonDocument.Parse(RepositoryContext.Read("native/TFusion.Kernel.Native/vcpkg.json"));
        var root = manifest.RootElement;
        Assert.Equal("04a9d8e5212d01ee1dd9478eadd9caade4f8b0d4", root.GetProperty("builtin-baseline").GetString());

        var occt = root.GetProperty("dependencies").EnumerateArray()
            .Single(dependency => dependency.GetProperty("name").GetString() == "opencascade");
        Assert.False(occt.GetProperty("default-features").GetBoolean());
        Assert.Equal("windows & x64", occt.GetProperty("platform").GetString());

        var occtOverride = root.GetProperty("overrides").EnumerateArray()
            .Single(dependency => dependency.GetProperty("name").GetString() == "opencascade");
        Assert.Equal("8.0.1", occtOverride.GetProperty("version").GetString());
    }

    [Fact]
    public void M2A06PublicNativeHeaderContainsOnlyCAbiTypes()
    {
        var header = RepositoryContext.Read("native/TFusion.Kernel.Native/include/tfusion_kernel.h");
        Assert.Contains("extern \"C\"", header, StringComparison.Ordinal);
        Assert.Contains("TFUSION_CALL __cdecl", header, StringComparison.Ordinal);
        Assert.Contains("uint64_t TFusionHandle", header, StringComparison.Ordinal);
        Assert.Contains("structSize", header, StringComparison.Ordinal);
        Assert.Contains("structVersion", header, StringComparison.Ordinal);

        var forbidden = new[]
        {
            "std::", "TopoDS_", "Handle(", "Standard_", "template<", "std::string", "std::vector",
        };
        Assert.All(forbidden, value => Assert.DoesNotContain(value, header, StringComparison.Ordinal));
    }

    [Fact]
    public void M2A07EveryNativeExportUsesTheExceptionBoundary()
    {
        var header = RepositoryContext.Read("native/TFusion.Kernel.Native/include/tfusion_kernel.h");
        var implementation = RepositoryContext.Read("native/TFusion.Kernel.Native/src/abi.cpp");
        var exportCount = header.Split('\n').Count(line => line.StartsWith("TFUSION_API TFusionStatus", StringComparison.Ordinal));
        var boundaryCount = implementation.Split("tfusion::protect(", StringSplitOptions.None).Length - 1;
        Assert.True(exportCount > 0);
        Assert.Equal(exportCount, boundaryCount);
        Assert.Contains("catch (const Standard_Failure&", RepositoryContext.Read("native/TFusion.Kernel.Native/src/diagnostic.hpp"), StringComparison.Ordinal);
        Assert.Contains("catch (const std::exception&", RepositoryContext.Read("native/TFusion.Kernel.Native/src/diagnostic.hpp"), StringComparison.Ordinal);
        Assert.Contains("catch (...)", RepositoryContext.Read("native/TFusion.Kernel.Native/src/diagnostic.hpp"), StringComparison.Ordinal);
    }

    [Fact]
    public void M2A08NativeMilestoneDoesNotStartGeometryOrExchangeFeatures()
    {
        var nativeSource = string.Join('\n', RepositoryContext.Files("*", SearchOption.AllDirectories)
            .Where(path => path.Contains(
                $"{Path.DirectorySeparatorChar}native{Path.DirectorySeparatorChar}TFusion.Kernel.Native{Path.DirectorySeparatorChar}",
                StringComparison.Ordinal)
                && Path.GetExtension(path) is ".h" or ".hpp" or ".c" or ".cpp")
            .Select(File.ReadAllText));
        var forbidden = new[]
        {
            "BRepPrimAPI", "BRepAlgoAPI", "STEPControl", "IGESControl", "StlAPI", "BRepMesh", "TopoDS_Shape",
        };
        Assert.All(forbidden, value => Assert.DoesNotContain(value, nativeSource, StringComparison.Ordinal));
    }

    [Fact]
    public void M1U12AppLoggingIsBoundedLocalAndHasNoNetworkSink()
    {
        var compositionRoot = RepositoryContext.Read("src/TFusion.App/CompositionRoot.cs");
        var appProject = RepositoryContext.Read("src/TFusion.App/TFusion.App.csproj");

        Assert.Contains("WriteTo.File", compositionRoot, StringComparison.Ordinal);
        Assert.Contains("fileSizeLimitBytes: policy.FileSizeLimitBytes", compositionRoot, StringComparison.Ordinal);
        Assert.Contains("retainedFileCountLimit: policy.RetainedFileCountLimit", compositionRoot, StringComparison.Ordinal);
        Assert.Contains("Path.Combine(paths.Logs", compositionRoot, StringComparison.Ordinal);
        Assert.DoesNotContain("WriteTo.Http", compositionRoot, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteTo.Seq", compositionRoot, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("http", appProject, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("telemetry", appProject, StringComparison.OrdinalIgnoreCase);
    }

    [GeneratedRegex(@"^\d+\.\d+\.\d+(?:\.\d+)?$", RegexOptions.CultureInvariant)]
    private static partial Regex ExactVersionRegex();

    [GeneratedRegex(@"^\s*uses:\s+[^\s@]+@[0-9a-f]{40}\s+#\s+.+$", RegexOptions.CultureInvariant)]
    private static partial Regex ActionPinRegex();
}
