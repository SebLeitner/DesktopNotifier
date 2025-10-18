using System;
using System.Windows;
using DesktopNotifier.Configuration;
using DesktopNotifier.Logging;
using DesktopNotifier.Services;
using DesktopNotifier.ViewModels;

namespace DesktopNotifier;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        AppSettings settings = ((App)Application.Current).Settings;
        Logger.LogInfo("Initialisiere Hauptfenster.");

        NotificationToastService toastService = new();

        MainViewModel? viewModel = null;
        NotificationClient client = new(settings.ApiBaseUrl, settings.ClientId);
        NotificationPoller poller = new(
            client,
            TimeSpan.FromSeconds(Math.Max(3, settings.PollingIntervalSeconds)),
            messages => viewModel!.HandleNotificationsAsync(messages));

        viewModel = new MainViewModel(poller, toastService);
        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += OnLoaded;
        Closed += (_, _) => _viewModel.Dispose();

        Logger.LogInfo("Hauptfenster vollständig initialisiert.");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Logger.LogInfo("MainWindow geladen, starte Poller.");
        _viewModel.Start();

        AppSettings settings = ((App)Application.Current).Settings;
        if (!settings.ShowDebugWindowOnStartup)
        {
            Hide();
            Logger.LogDebug("Fenster versteckt, da ShowDebugWindowOnStartup = false.");
        }
    }

    private void OnStart(object sender, RoutedEventArgs e)
    {
        Logger.LogInfo("Start-Schaltfläche betätigt.");
        _viewModel.Start();
    }

    private void OnStop(object sender, RoutedEventArgs e)
    {
        Logger.LogInfo("Stop-Schaltfläche betätigt.");
        _viewModel.Stop();
    }

    private static FileLogger Logger => ((App)Application.Current).Logger;
}
