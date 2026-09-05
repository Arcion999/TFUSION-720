using System.Runtime.InteropServices;

namespace TFusion.Kernel.Interop.Native;

internal static class NativeMethods
{
    internal const string LibraryName = "TFusion.Kernel.Native.dll";
    internal const uint AbiVersion = 1;
    internal const uint ContextCreateInfoVersion = 1;
    internal const uint KernelInfoVersion = 1;
    internal const uint AllocationSnapshotVersion = 1;

    static NativeMethods() => NativeBridgeLoader.Register();

    [DllImport(LibraryName, EntryPoint = "tfusion_get_abi_version", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus GetAbiVersion(out uint abiVersion);

    [DllImport(LibraryName, EntryPoint = "tfusion_negotiate_abi", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus NegotiateAbi(uint requestedAbiVersion, out uint negotiatedAbiVersion);

    [DllImport(LibraryName, EntryPoint = "tfusion_context_create", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ContextCreate(in NativeContextCreateInfo createInfo, out ulong contextHandle);

    [DllImport(LibraryName, EntryPoint = "tfusion_context_destroy", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ContextDestroy(ulong contextHandle);

    [DllImport(LibraryName, EntryPoint = "tfusion_context_get_kernel_info", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ContextGetKernelInfo(ulong contextHandle, ref NativeKernelInfo kernelInfo);

    [DllImport(LibraryName, EntryPoint = "tfusion_context_set_client_name_utf8", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ContextSetClientNameUtf8(
        ulong contextHandle,
        [In] byte[]? value,
        uint valueByteCount);

    [DllImport(LibraryName, EntryPoint = "tfusion_context_get_text_utf8", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ContextGetTextUtf8(
        ulong contextHandle,
        NativeTextField field,
        [Out] byte[]? buffer,
        uint bufferByteCount,
        out uint requiredByteCount);

    [DllImport(LibraryName, EntryPoint = "tfusion_probe_create", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ProbeCreate(ulong contextHandle, out ulong probeHandle);

    [DllImport(LibraryName, EntryPoint = "tfusion_probe_get_runtime_version_utf8", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ProbeGetRuntimeVersionUtf8(
        ulong contextHandle,
        ulong probeHandle,
        [Out] byte[]? buffer,
        uint bufferByteCount,
        out uint requiredByteCount);

    [DllImport(LibraryName, EntryPoint = "tfusion_probe_release", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus ProbeRelease(ulong contextHandle, ulong probeHandle);

    [DllImport(LibraryName, EntryPoint = "tfusion_test_exception_boundary", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus TestExceptionBoundary(ulong contextHandle, NativeExceptionProbe exceptionProbe);

    [DllImport(LibraryName, EntryPoint = "tfusion_debug_get_allocation_snapshot", CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
    internal static extern NativeStatus DebugGetAllocationSnapshot(ref NativeAllocationSnapshot snapshot);
}
