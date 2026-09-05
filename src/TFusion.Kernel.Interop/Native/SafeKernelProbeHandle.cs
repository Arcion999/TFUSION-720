using Microsoft.Win32.SafeHandles;

namespace TFusion.Kernel.Interop.Native;

internal sealed class SafeKernelProbeHandle : SafeHandleZeroOrMinusOneIsInvalid
{
    private SafeKernelContextHandle? owner;
    private bool ownerReferenceAdded;

    internal SafeKernelProbeHandle()
        : base(ownsHandle: true)
    {
    }

    internal static SafeKernelProbeHandle Create(SafeKernelContextHandle owner, ulong value)
    {
        ArgumentNullException.ThrowIfNull(owner);
        var result = new SafeKernelProbeHandle
        {
            owner = owner,
        };
        var added = false;
        owner.DangerousAddRef(ref added);
        result.ownerReferenceAdded = added;
        result.SetHandle(new nint(unchecked((long)value)));
        return result;
    }

    internal ulong Value => unchecked((ulong)DangerousGetHandle().ToInt64());

    protected override bool ReleaseHandle()
    {
        try
        {
            if (owner is null)
            {
                return false;
            }

            try
            {
                var status = NativeMethods.ProbeRelease(owner.Value, unchecked((ulong)handle.ToInt64()));
                return status is NativeStatus.Success or NativeStatus.StaleHandle;
            }
            catch (Exception exception) when (KernelDiagnosticFactory.IsInteropException(exception))
            {
                return false;
            }
        }
        finally
        {
            if (ownerReferenceAdded)
            {
                owner!.DangerousRelease();
                ownerReferenceAdded = false;
            }
            owner = null;
        }
    }
}
