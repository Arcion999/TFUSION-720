using System.Windows;
using System.Windows.Threading;
using Serilog;

namespace TFusion.App;

public partial class App : Application
{
    private CompositionRoot? compositionRoot;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var compositionResult = CompositionRoot.Create();
        if (compositionResult.IsFailure)
        {
            var message = compositionResult.Diagnostics[0].UserMessage;
            MessageBox.Show(message, "TFUSION-720 startup failure", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
            return;
        }

        compositionRoot = compositionResult.Value;
        RegisterExceptionHandlers();

        try
        {
            compositionRoot.Start();
            var observationResult = compositionRoot.StartupSentinel.BeginSession(
                compositionRoot.SessionId,
                DateTimeOffset.UtcNow);

            if (observationResult.IsFailure)
            {
                Log.Fatal("Startup sentinel initialization failed: {@Diagnostics}", observationResult.Diagnostics);
                MessageBox.Show(
                    observationResult.Diagnostics[0].UserMessage,
                    "TFUSION-720 startup failure",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
                return;
            }

            var observation = observationResult.Value;
            if (observation.PreviousSessionWasUnclean || observation.PreviousMarkerWasInvalid)
            {
                Log.Warning(
                    "Previous session state required attention. Unclean={Unclean}; InvalidMarker={InvalidMarker}",
                    observation.PreviousSessionWasUnclean,
                    observation.PreviousMarkerWasInvalid);
            }

            Log.Information("Application started");
            MainWindow = compositionRoot.MainWindow;
            MainWindow.Show();
        }
        catch (Exception exception)
        {
            Log.Fatal(exception, "Application startup failed");
            MessageBox.Show(
                "TFUSION-720 could not start. See the local application log for details.",
                "TFUSION-720 startup failure",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (compositionRoot is not null)
        {
            var cleanResult = compositionRoot.StartupSentinel.MarkClean(
                compositionRoot.SessionId,
                DateTimeOffset.UtcNow);
            if (cleanResult.IsFailure)
            {
                Log.Error("Clean shutdown marker failed: {@Diagnostics}", cleanResult.Diagnostics);
            }

            try
            {
                compositionRoot.Stop();
            }
            catch (Exception exception)
            {
                Log.Error(exception, "Application host shutdown failed");
            }

            compositionRoot.Dispose();
        }

        Log.Information("Application stopped with exit code {ExitCode}", e.ApplicationExitCode);
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private void RegisterExceptionHandlers()
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnDispatcherUnhandledException(
        object sender,
        DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unrecoverable dispatcher exception");
        e.Handled = true;
        Shutdown(1);
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.ExceptionObject as Exception, "Unhandled managed exception");
        Log.CloseAndFlush();
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unobserved task exception");
        e.SetObserved();
    }
}
