using TFusion.Foundation.Results;
using TFusion.Kernel.Interop.Native;

namespace TFusion.Kernel.Interop;

public static class KernelBridge
{
    public const uint SupportedAbiVersion = NativeMethods.AbiVersion;

    public static Result<uint> QueryAbiVersion()
    {
        try
        {
            var status = NativeMethods.GetAbiVersion(out var version);
            return status == NativeStatus.Success
                ? Result.Success(version)
                : Result.Failure<uint>(KernelDiagnosticFactory.FromStatus(status, 0, nameof(QueryAbiVersion)));
        }
        catch (Exception exception) when (KernelDiagnosticFactory.IsInteropException(exception))
        {
            return Result.Failure<uint>(KernelDiagnosticFactory.LoadFailure(exception));
        }
    }

    public static Result<uint> NegotiateAbiVersion(uint requestedVersion)
    {
        try
        {
            var status = NativeMethods.NegotiateAbi(requestedVersion, out var negotiated);
            return status == NativeStatus.Success
                ? Result.Success(negotiated)
                : Result.Failure<uint>(KernelDiagnosticFactory.FromStatus(status, 0, nameof(NegotiateAbiVersion)));
        }
        catch (Exception exception) when (KernelDiagnosticFactory.IsInteropException(exception))
        {
            return Result.Failure<uint>(KernelDiagnosticFactory.LoadFailure(exception));
        }
    }

    public static Result<KernelContext> CreateContext(string clientName) => KernelContext.Create(clientName);
}
