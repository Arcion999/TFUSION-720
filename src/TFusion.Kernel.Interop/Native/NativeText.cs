using System.Text;

namespace TFusion.Kernel.Interop.Native;

internal static class NativeText
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal delegate NativeStatus TextReader(byte[]? buffer, uint bufferSize, out uint requiredSize);

    internal static NativeStatus Read(TextReader reader, out string value)
    {
        ArgumentNullException.ThrowIfNull(reader);
        value = string.Empty;
        var status = reader(null, 0, out var required);
        if (status != NativeStatus.BufferTooSmall || required == 0 || required > 1024 * 1024)
        {
            return status == NativeStatus.Success ? NativeStatus.InternalError : status;
        }

        var buffer = new byte[required];
        status = reader(buffer, required, out var returnedRequired);
        if (status != NativeStatus.Success || returnedRequired != required || buffer[^1] != 0)
        {
            return status == NativeStatus.Success ? NativeStatus.InternalError : status;
        }

        try
        {
            value = StrictUtf8.GetString(buffer, 0, buffer.Length - 1);
            return NativeStatus.Success;
        }
        catch (DecoderFallbackException)
        {
            value = string.Empty;
            return NativeStatus.InternalError;
        }
    }
}
