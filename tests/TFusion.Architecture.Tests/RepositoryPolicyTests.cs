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
    public void M1A11ReadmeMakesNoSupportedCadCapabilityClaim()
    {
        var readme = RepositoryContext.Read("README.md");
        Assert.Contains("contains no CAD functionality", readme, StringComparison.OrdinalIgnoreCase);
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
    }

    [Fact]
    public void M1U12AppLoggingIsBoundedLocalAndHasNoNetworkSink()
    {
        var compositionRoot = RepositoryContext.Read("src/TFusion.App/CompositionRoot.cs");
        var appProject = RepositoryContext.Read("src/TFusion.App/TFusion.App.csproj");

        Assert.Contains("WriteTo.File", compositionRoot, StringComparison.Ordinal);
        Assert.Contains("fileSizeLimitBytes: policy.FileSizeLimitBytes", compositionRoot, StringComparison.Ordinal);
        Assert.Contains("retainedFileCountLimit: policy.RetainedFileCountLimit", compositionRoot, StringComparison.Ordinal);
        Assert.Contains("Paths.Logs", compositionRoot, StringComparison.Ordinal);
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
