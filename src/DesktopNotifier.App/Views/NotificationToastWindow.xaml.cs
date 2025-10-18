using System.Windows;
using DesktopNotifier.Models;

namespace DesktopNotifier.Views;

public partial class NotificationToastWindow : Window
{
    public NotificationToastWindow(NotificationMessage message)
    {
        InitializeComponent();
        DataContext = message;
    }
}
