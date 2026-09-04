using System.Reflection.Emit;
namespace TFusion.Foundation.Tests;

public sealed class BuildInfoTests
{
    [Fact]
    public void M1U11CurrentBuildInfoIsPresentAndParseable()
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
        var assemblyName = new System.Reflection.AssemblyName("TFusion.MetadataFreeTestAssembly")
        {
            Version = null,
        };
        var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var buildInfo = BuildInfo.FromAssembly(assembly);

        Assert.Equal("0.0.0.0", buildInfo.ProductVersion);
        Assert.Equal("unknown", buildInfo.InformationalVersion);
    }
}
