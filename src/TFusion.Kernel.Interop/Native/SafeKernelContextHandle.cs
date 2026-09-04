using Microsoft.Win32.SafeHandles;

namespace TFusion.Kernel.Interop.Native;

internal sealed class SafeKernelContextHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeKernelContextHandle()
        : base(ownsHandle: true)
    {
    }

    internal static SafeKernelContextHandle Create(ulong value)
    {
        var result = new SafeKernelContextHandle();
        result.SetHandle(new nint(unchecked((long)value)));
        return result;
    }

    internal ulong Value => unchecked((ulong)DangerousGetHandle().ToInt64());

    protected override bool ReleaseHandle()
    {
        try
        {
            var status = NativeMethods.ContextDestroy(unchecked((ulong)handle.ToInt64()));
            return status is NativeStatus.Success or NativeStatus.StaleHandle;
        }
        catch (Exception exception) when (KernelDiagnosticFactory.IsInteropException(exception))
        {
            return false;
        }
    }
}
