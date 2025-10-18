using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using DesktopNotifier.Models;
using DesktopNotifier.Services;

namespace DesktopNotifier.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly NotificationPoller _poller;
    private readonly NotificationToastService _toastService;

    private bool _isRunning;

    public ObservableCollection<NotificationMessage> Notifications { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel(NotificationPoller poller, NotificationToastService toastService)
    {
        _poller = poller;
        _toastService = toastService;
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set
        {
            if (_isRunning == value)
            {
                return;
            }

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    public void Start()
    {
        if (IsRunning)
        {
            return;
        }

        _poller.Start();
        IsRunning = true;
    }

    public void Stop()
    {
        if (!IsRunning)
        {
            return;
        }

        _poller.Stop();
        IsRunning = false;
    }

    public async Task HandleNotificationsAsync(System.Collections.Generic.IReadOnlyList<NotificationMessage> messages)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (NotificationMessage message in messages)
            {
                Notifications.Insert(0, message);
                _toastService.ShowToast(message);
            }
        });
    }

    public void Dispose()
    {
        Stop();
        _poller.Dispose();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
