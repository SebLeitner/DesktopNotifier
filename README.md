# DesktopNotifier

Ein leichtgewichtiges WPF-Tool, das in regelmäßigen Abständen eine HTTP-API abfragt und neue Benachrichtigungen sowohl in einem Desktopfenster als auch als kleines Overlay (Toast) anzeigt. Das Projekt ist auf Windows 10/11 ausgelegt und lässt sich mit dem .NET 8 SDK bauen.

## Projektstruktur

```
DesktopNotifier/
└── src/
    └── DesktopNotifier.App/
        ├── DesktopNotifier.App.csproj
        ├── App.xaml / App.xaml.cs
        ├── MainWindow.xaml / MainWindow.xaml.cs
        ├── Views/NotificationToastWindow.*
        ├── Services/* (API-Client, Poller, Toast-Service)
        ├── ViewModels/*
        ├── Converters/*
        └── appsettings.json
```

## Konfiguration

Die Datei `appsettings.json` (wird beim Build automatisch in den Output kopiert) steuert das Verhalten des Clients:

```json
{
  "ApiBaseUrl": "https://your-api-gateway-id.execute-api.eu-central-1.amazonaws.com/prod/",
  "ClientId": "windows-client-001",
  "PollingIntervalSeconds": 10,
  "ShowDebugWindowOnStartup": true
}
```

| Einstellung | Beschreibung |
|-------------|--------------|
| `ApiBaseUrl` | Basisadresse deiner REST-API. Der Client ruft `GET {ApiBaseUrl}/notifications` auf und erwartet eine JSON-Liste von Benachrichtigungen. |
| `ClientId` | Wird als `X-Client-Id`-Header mitgesendet, um serverseitig Clients unterscheiden zu können. |
| `PollingIntervalSeconds` | Abfrageintervall (mindestens 3 Sekunden). |
| `ShowDebugWindowOnStartup` | Wenn `false`, startet der Client im Hintergrund und zeigt nur die Toasts an. |

Die API sollte ein Array aus Objekten liefern, das mindestens `text`, `topic`, `application` und optional `id` sowie `createdAt` (ISO-8601) enthält. Beispiel:

```json
[
  {
    "id": "81362d9d-6686-4c35-bf9c-72be4f5bf5be",
    "application": "CRM",
    "topic": "Neuer Lead",
    "text": "Lead Müller wurde zugewiesen",
    "createdAt": "2024-05-05T09:15:42Z"
  }
]
```

## Voraussetzungen

- Windows 10 oder 11
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download) (enthält MSBuild und `dotnet` CLI)

> **Hinweis:** Für WPF-Builds muss MSBuild auf einem Windows-System ausgeführt werden. Unter Linux/macOS ist der Build nicht möglich.

## Build & Start

1. Repository klonen:
   ```powershell
   git clone https://github.com/<dein-account>/DesktopNotifier.git
   cd DesktopNotifier
   ```

2. Abhängigkeiten laden und bauen:
   ```powershell
   dotnet restore .\src\DesktopNotifier.App\DesktopNotifier.App.csproj
   dotnet build .\src\DesktopNotifier.App\DesktopNotifier.App.csproj -c Release
   ```

3. Starten (Debug):
   ```powershell
   dotnet run --project .\src\DesktopNotifier.App\DesktopNotifier.App.csproj
   ```

4. Für die Verteilung ein selbstenthaltendes Paket bauen (optional):
   ```powershell
   dotnet publish .\src\DesktopNotifier.App\DesktopNotifier.App.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o .\publish
   ```
   Das Ergebnis liegt anschließend im Ordner `publish`. Die `appsettings.json` kann dort angepasst werden.

## Funktionsweise

- Beim Start lädt der Client die Einstellungen, initialisiert den HTTP-Client und beginnt mit dem Polling.
- Neue Benachrichtigungen erscheinen als Einträge im Hauptfenster (Liste) und zusätzlich als kleine Overlays am unteren rechten Bildschirmrand.
- Über die Buttons im Fenster lässt sich das Polling manuell starten oder stoppen.

## Weiteres Vorgehen

- Tray-Icon und Autostart ergänzen, damit das Tool komplett im Hintergrund läuft.
- Fehlerlogging (z. B. Serilog) für Analyse/Support.
- Quittierung neuer Nachrichten via API (z. B. `POST /notifications/{id}/ack`).

Viel Erfolg beim Testen und Erweitern!
