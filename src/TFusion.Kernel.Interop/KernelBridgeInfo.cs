namespace TFusion.Kernel.Interop;

public sealed record KernelBridgeInfo(
    uint AbiVersion,
    string CompiledOcctVersion,
    string RuntimeOcctVersion,
    string Architecture,
    string NativeBridgePath);
