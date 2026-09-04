using System.Runtime.InteropServices;

namespace TFusion.Kernel.Interop.Native;

internal enum NativeStatus
{
    Success = 0,
    InvalidArgument = 1,
    InvalidHandle = 2,
    StaleHandle = 3,
    TypeMismatch = 4,
    ContextMismatch = 5,
    BufferTooSmall = 6,
    VersionMismatch = 7,
    InternalError = 8,
    KernelError = 9,
    Unsupported = 10,
}

internal enum NativeTextField : uint
{
    CompiledOcctVersion = 1,
    RuntimeOcctVersion = 2,
    ContextClientName = 3,
    LastDiagnosticJson = 4,
}

internal enum NativeExceptionProbe : uint
{
    Occt = 1,
    Standard = 2,
    Unknown = 3,
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeContextCreateInfo
{
    internal uint StructSize;
    internal uint StructVersion;
    internal uint RequestedAbiVersion;
    internal nint ClientNameUtf8;
    internal uint ClientNameByteCount;
    internal uint Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeKernelInfo
{
    internal uint StructSize;
    internal uint StructVersion;
    internal uint AbiVersion;
    internal uint Architecture;
    internal uint CompiledOcctVersionRequiredBytes;
    internal uint RuntimeOcctVersionRequiredBytes;
}

[StructLayout(LayoutKind.Sequential)]
internal struct NativeAllocationSnapshot
{
    internal uint StructSize;
    internal uint StructVersion;
    internal ulong ActiveContexts;
    internal ulong ActiveProbes;
    internal ulong TotalAllocations;
    internal ulong TotalReleases;
}
