using System.Reflection;

namespace TFusion.Foundation;

public sealed record BuildInfo(string ProductVersion, string InformationalVersion)
{
    public static BuildInfo Current { get; } = FromAssembly(typeof(BuildInfo).Assembly);

    public static BuildInfo FromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var assemblyVersion = assembly.GetName().Version?.ToString();
        var informationalVersion = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        return new BuildInfo(
            string.IsNullOrWhiteSpace(assemblyVersion) ? "0.0.0.0" : assemblyVersion,
            string.IsNullOrWhiteSpace(informationalVersion) ? "unknown" : informationalVersion);
    }
}
