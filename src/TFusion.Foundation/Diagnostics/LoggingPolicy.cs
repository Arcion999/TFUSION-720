namespace TFusion.Foundation.Diagnostics;

public sealed record LoggingPolicy
{
    public const long DefaultFileSizeLimitBytes = 10 * 1024 * 1024;
    public const int DefaultRetainedFileCountLimit = 14;

    public LoggingPolicy(long fileSizeLimitBytes, int retainedFileCountLimit)
    {
        if (fileSizeLimitBytes <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fileSizeLimitBytes));
        }

        if (retainedFileCountLimit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(retainedFileCountLimit));
        }

        FileSizeLimitBytes = fileSizeLimitBytes;
        RetainedFileCountLimit = retainedFileCountLimit;
    }

    public static LoggingPolicy Default { get; } =
        new(DefaultFileSizeLimitBytes, DefaultRetainedFileCountLimit);

    public long FileSizeLimitBytes { get; }

    public int RetainedFileCountLimit { get; }
}
