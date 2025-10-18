using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace DesktopNotifier.Logging;

public sealed class FileLogger : IDisposable
{
    private readonly string _filePath;
    private readonly object _syncRoot = new();
    private readonly Encoding _encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    private bool _disposed;

    public FileLogger(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

        string? directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        WriteInternal("Logger initialisiert");
    }

    public void LogInfo(string message)
    {
        WriteInternal(message, "INFO");
    }

    public void LogError(string message, Exception? exception = null)
    {
        var builder = new StringBuilder(message);
        if (exception is not null)
        {
            builder.AppendLine();
            builder.Append(exception);
        }

        WriteInternal(builder.ToString(), "ERROR");
    }

    public void LogDebug(string message)
    {
        WriteInternal(message, "DEBUG");
    }

    private void WriteInternal(string message, string level = "INFO")
    {
        if (_disposed)
        {
            return;
        }

        string line = string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}",
            DateTime.Now, level, message);

        lock (_syncRoot)
        {
            File.AppendAllText(_filePath, line + Environment.NewLine, _encoding);
        }
    }

    public void Dispose()
    {
        _disposed = true;
    }
}
