using System.Reflection;
using TFusion.Foundation;

namespace TFusion.Foundation.Tests;

public sealed class BuildInfoTests
{
    [Fact]
    public void M1_U11_CurrentBuildInfoIsPresentAndParseable()
    {
        var buildInfo = BuildInfo.Current;

        Assert.True(Version.TryParse(buildInfo.ProductVersion, out _));
        Assert.False(string.IsNullOrWhiteSpace(buildInfo.InformationalVersion));
    }

    [Fact]
    public void BuildInfoRejectsNullAssembly()
    {
        Assert.Throws<ArgumentNullException>(() => BuildInfo.FromAssembly(null!));
    }

    [Fact]
    public void BuildInfoUsesSafeFallbacksWhenMetadataIsAbsent()
    {
        var buildInfo = BuildInfo.FromAssembly(typeof(string).Assembly);

        Assert.True(Version.TryParse(buildInfo.ProductVersion, out _));
        Assert.False(string.IsNullOrWhiteSpace(buildInfo.InformationalVersion));
    }
}
