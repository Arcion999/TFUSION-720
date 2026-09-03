using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using TFusion.Foundation;
using TFusion.Foundation.Diagnostics;
using TFusion.Foundation.Lifecycle;
using TFusion.Foundation.Results;
using TFusion.Foundation.Storage;

namespace TFusion.App;

public sealed class CompositionRoot : IDisposable
{
    private readonly IHost host;
    private bool disposed;

    private CompositionRoot(
        IHost host,
        ProductPaths paths,
        StartupSentinel startupSentinel,
        Guid sessionId)
    {
        this.host = host;
        Paths = paths;
        StartupSentinel = startupSentinel;
        SessionId = sessionId;
    }

    public ProductPaths Paths { get; }

    public StartupSentinel StartupSentinel { get; }

    public Guid SessionId { get; }

    public MainWindow MainWindow => host.Services.GetRequiredService<MainWindow>();

    public static Result<CompositionRoot> Create()
    {
        var pathsResult = ProductPaths.CreateDefault();
        if (pathsResult.IsFailure)
        {
            return Result.Failure<CompositionRoot>(pathsResult.Diagnostics);
        }

        var paths = pathsResult.Value;
        var createDirectoriesResult = paths.EnsureCreated();
        if (createDirectoriesResult.IsFailure)
        {
            return Result.Failure<CompositionRoot>(createDirectoriesResult.Diagnostics);
        }

        var sessionId = Guid.NewGuid();
        var policy = LoggingPolicy.Default;
        var logPath = Path.Combine(paths.Logs, "tfusion-.jsonl");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.WithProperty("Application", "TFUSION-720")
            .Enrich.WithProperty("ApplicationVersion", BuildInfo.Current.InformationalVersion)
            .Enrich.WithProperty("ProcessId", Environment.ProcessId)
            .Enrich.WithProperty("SessionId", sessionId)
            .Enrich.With(new ManagedThreadIdEnricher())
            .WriteTo.File(
                new UtcJsonFormatter(),
                logPath,
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: policy.FileSizeLimitBytes,
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: policy.RetainedFileCountLimit,
                shared: false)
            .CreateLogger();

        try
        {
            var host = Host.CreateDefaultBuilder([])
                .UseSerilog(Log.Logger, dispose: false)
                .ConfigureServices(services => services.AddSingleton<MainWindow>())
                .Build();

            return Result.Success(new CompositionRoot(
                host,
                paths,
                new StartupSentinel(paths.StartupSentinelFile),
                sessionId));
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Application composition failed");
            Log.CloseAndFlush();
            return Result.Failure<CompositionRoot>(new CadDiagnostic(
                FoundationDiagnosticCodes.InvalidConfiguration,
                DiagnosticSeverity.Fatal,
                "TFUSION-720 could not initialize.",
                exception.Message));
        }
    }

    public void Start() => host.Start();

    public void Stop() => host.StopAsync(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        host.Dispose();
        disposed = true;
    }

    private sealed class ManagedThreadIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            ArgumentNullException.ThrowIfNull(logEvent);
            ArgumentNullException.ThrowIfNull(propertyFactory);
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                "ManagedThreadId",
                Environment.CurrentManagedThreadId));
        }
    }
}
