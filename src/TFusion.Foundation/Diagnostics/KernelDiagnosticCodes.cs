namespace TFusion.Foundation.Diagnostics;

public static class KernelDiagnosticCodes
{
    public static readonly DiagnosticCode NativeBridgeLoadFailed = new("TFN-KRN-LOAD");
    public static readonly DiagnosticCode InvalidArgument = new("TFN-KRN-0001");
    public static readonly DiagnosticCode InvalidHandle = new("TFN-KRN-0002");
    public static readonly DiagnosticCode StaleHandle = new("TFN-KRN-0003");
    public static readonly DiagnosticCode TypeMismatch = new("TFN-KRN-0004");
    public static readonly DiagnosticCode ContextMismatch = new("TFN-KRN-0005");
    public static readonly DiagnosticCode BufferTooSmall = new("TFN-KRN-0006");
    public static readonly DiagnosticCode VersionMismatch = new("TFN-KRN-0007");
    public static readonly DiagnosticCode InternalError = new("TFN-KRN-0008");
    public static readonly DiagnosticCode KernelError = new("TFN-KRN-0009");
    public static readonly DiagnosticCode Unsupported = new("TFN-KRN-0010");
}
