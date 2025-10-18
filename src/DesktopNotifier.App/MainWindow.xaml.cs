using System;
using System.Windows;
using DesktopNotifier.Configuration;
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
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _viewModel.Start();

        AppSettings settings = ((App)Application.Current).Settings;
        if (!settings.ShowDebugWindowOnStartup)
        {
            Hide();
        }
    }

    private void OnStart(object sender, RoutedEventArgs e)
    {
        _viewModel.Start();
    }

    private void OnStop(object sender, RoutedEventArgs e)
    {
        _viewModel.Stop();
    }
}
