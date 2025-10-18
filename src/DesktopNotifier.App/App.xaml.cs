using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DesktopNotifier.Configuration;
using DesktopNotifier.Logging;

namespace DesktopNotifier;

public partial class App : Application
{
    public AppSettings Settings { get; private set; } = null!;
    public FileLogger Logger { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        InitializeLogger();
        Logger.LogInfo("Anwendung startet.");

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            Settings = AppSettings.LoadFromFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
            Logger.LogInfo("Konfiguration erfolgreich geladen.");
        }
        catch (Exception ex)
        {
            Logger.LogError("Die Konfigurationsdatei konnte nicht geladen werden.", ex);
            MessageBox.Show($"Die Konfigurationsdatei konnte nicht geladen werden: {ex.Message}",
                "DesktopNotifier", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Logger.LogInfo("Anwendung wird beendet.");

        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

        base.OnExit(e);

        Logger.Dispose();
    }

    private void InitializeLogger()
    {
        string logDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DesktopNotifier");
        Directory.CreateDirectory(logDirectory);

        string logFilePath = Path.Combine(logDirectory, "desktop-notifier.log");
        Logger = new FileLogger(logFilePath);
        Logger.LogInfo("Dateilogger initialisiert.");
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Logger.LogError("Dispatcher-Ausnahme aufgetreten.", e.Exception);
        e.Handled = true;

        MessageBox.Show($"Ein unerwarteter Fehler ist aufgetreten: {e.Exception.Message}",
            "DesktopNotifier", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            Logger.LogError("Nicht abgefangene Ausnahme im AppDomain-Kontext.", ex);
        }
        else
        {
            string description = e.ExceptionObject is null
                ? "(null)"
                : e.ExceptionObject.GetType().FullName;
            Logger.LogError($"Nicht abgefangene Ausnahme vom Typ {description}.");
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Logger.LogError("Nicht beobachtete Task-Ausnahme.", e.Exception);
        e.SetObserved();
    }
}
