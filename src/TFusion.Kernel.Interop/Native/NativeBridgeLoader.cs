using System.Reflection;
using System.Runtime.InteropServices;

namespace TFusion.Kernel.Interop.Native;

internal static class NativeBridgeLoader
{
    private static int registered;
    private static readonly object Sync = new();
    private static nint loadedHandle;

    internal static string ExpectedPath => Path.Combine(AppContext.BaseDirectory, NativeMethods.LibraryName);

    internal static void Register()
    {
        if (Interlocked.Exchange(ref registered, 1) == 0)
        {
            NativeLibrary.SetDllImportResolver(typeof(NativeBridgeLoader).Assembly, Resolve);
        }
    }

    private static nint Resolve(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
    {
        _ = assembly;
        _ = searchPath;
        if (!string.Equals(libraryName, NativeMethods.LibraryName, StringComparison.Ordinal))
        {
            return 0;
        }

        var absolutePath = ExpectedPath;
        if (!File.Exists(absolutePath))
        {
            throw new DllNotFoundException($"Required native bridge was not packaged at '{absolutePath}'.");
        }

        lock (Sync)
        {
            if (loadedHandle == 0)
            {
                loadedHandle = NativeLibrary.Load(absolutePath);
            }
            return loadedHandle;
        }
    }
}
