using System.IO;
using Microsoft.Extensions.Configuration;

namespace DesktopNotifier.Configuration;

public sealed class AppSettings
{
    public required string ApiBaseUrl { get; init; }
    public required string ClientId { get; init; }
    public int PollingIntervalSeconds { get; init; } = 10;
    public bool ShowDebugWindowOnStartup { get; init; }

    public static AppSettings LoadFromFile(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Die Datei '{path}' wurde nicht gefunden.");
        }

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory())
            .AddJsonFile(Path.GetFileName(path), optional: false, reloadOnChange: false)
            .Build();

        AppSettings? settings = configuration.Get<AppSettings>();
        if (settings is null)
        {
            throw new InvalidDataException("Die appsettings.json konnte nicht in AppSettings konvertiert werden.");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiBaseUrl))
        {
            throw new InvalidDataException("ApiBaseUrl ist nicht konfiguriert.");
        }

        if (string.IsNullOrWhiteSpace(settings.ClientId))
        {
            throw new InvalidDataException("ClientId ist nicht konfiguriert.");
        }

        return settings;
    }
}
