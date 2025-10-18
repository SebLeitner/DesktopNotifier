using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using DesktopNotifier.Models;

namespace DesktopNotifier.Services;

public sealed class NotificationPoller : IDisposable
{
    private readonly NotificationClient _client;
    private readonly DispatcherTimer _timer;
    private readonly Func<IReadOnlyList<NotificationMessage>, Task> _onNotifications;
    private readonly HashSet<string> _seenNotificationIds = new();
    private bool _isRunning;

    public NotificationPoller(NotificationClient client, TimeSpan interval, Func<IReadOnlyList<NotificationMessage>, Task> onNotifications)
    {
        _client = client;
        _onNotifications = onNotifications;

        _timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = interval
        };

        _timer.Tick += OnTickAsync;
    }

    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;
        _timer.Start();
        _ = PollAsync();
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            return;
        }

        _timer.Stop();
        _isRunning = false;
    }

    private async void OnTickAsync(object? sender, EventArgs e)
    {
        await PollAsync();
    }

    private async Task PollAsync()
    {
        try
        {
            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(30));
            IReadOnlyList<NotificationMessage> notifications = await _client.FetchNotificationsAsync(cts.Token);

            List<NotificationMessage> unseen = new();
            foreach (NotificationMessage notification in notifications)
            {
                if (_seenNotificationIds.Add(notification.Id))
                {
                    unseen.Add(notification);
                }
            }

            if (unseen.Count > 0)
            {
                await _onNotifications(unseen);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    public void Dispose()
    {
        Stop();
        _timer.Tick -= OnTickAsync;
        _client.Dispose();
    }
}
