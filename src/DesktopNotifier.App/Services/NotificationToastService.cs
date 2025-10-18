using System;
using System.Threading.Tasks;
using System.Windows;
using DesktopNotifier.Models;
using DesktopNotifier.Views;

namespace DesktopNotifier.Services;

public sealed class NotificationToastService
{
    public void ShowToast(NotificationMessage message)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            NotificationToastWindow toast = new(message)
            {
                Left = SystemParameters.WorkArea.Right - 320,
                Top = SystemParameters.WorkArea.Bottom - 120,
                Topmost = true
            };

            toast.Show();
            _ = CloseAfterDelayAsync(toast, TimeSpan.FromSeconds(6));
        });
    }

    private static async Task CloseAfterDelayAsync(Window toast, TimeSpan delay)
    {
        await Task.Delay(delay);
        await toast.Dispatcher.InvokeAsync(() => toast.Close());
    }
}
