using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using DesktopNotifier.Models;
using DesktopNotifier.Logging;

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

        Logger.LogInfo($"Poller initialisiert. Intervall: {interval.TotalSeconds:F0}s");
    }

    public void Start()
    {
        if (_isRunning)
        {
            Logger.LogDebug("Start() ignoriert, Poller läuft bereits.");
            return;
        }

        _isRunning = true;
        _timer.Start();
        _ = PollAsync();

        Logger.LogInfo("Poller gestartet.");
    }

    public void Stop()
    {
        if (!_isRunning)
        {
            Logger.LogDebug("Stop() ignoriert, Poller läuft nicht.");
            return;
        }

        _timer.Stop();
        _isRunning = false;

        Logger.LogInfo("Poller gestoppt.");
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
                Logger.LogInfo($"{unseen.Count} neue Benachrichtigungen gefunden.");
                await _onNotifications(unseen);
            }
            else
            {
                Logger.LogDebug("Keine neuen Benachrichtigungen.");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError("Fehler beim Abfragen der Benachrichtigungen.", ex);
        }
    }

    public void Dispose()
    {
        Stop();
        _timer.Tick -= OnTickAsync;
        _client.Dispose();
        Logger.LogDebug("Poller freigegeben.");
    }

    private static FileLogger Logger => ((App)Application.Current).Logger;
}
