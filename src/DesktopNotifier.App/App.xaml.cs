using System;
using System.IO;
using System.Windows;
using DesktopNotifier.Configuration;

namespace DesktopNotifier;

public partial class App : Application
{
    public AppSettings Settings { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Settings = AppSettings.LoadFromFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Die Konfigurationsdatei konnte nicht geladen werden: {ex.Message}",
                "DesktopNotifier", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(-1);
        }
    }
}
