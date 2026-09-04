namespace TFusion.Foundation.Diagnostics;

public sealed record LoggingPolicy
{
    public const long DefaultFileSizeLimitBytes = 10 * 1024 * 1024;
    public const int DefaultRetainedFileCountLimit = 14;

    public LoggingPolicy(long fileSizeLimitBytes, int retainedFileCountLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSizeLimitBytes);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(retainedFileCountLimit);

        FileSizeLimitBytes = fileSizeLimitBytes;
        RetainedFileCountLimit = retainedFileCountLimit;
    }

    public static LoggingPolicy Default { get; } =
        new(DefaultFileSizeLimitBytes, DefaultRetainedFileCountLimit);

    public long FileSizeLimitBytes { get; }

    public int RetainedFileCountLimit { get; }
}
