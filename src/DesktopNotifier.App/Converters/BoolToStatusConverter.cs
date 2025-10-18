using System;
using System.Globalization;
using System.Windows.Data;

namespace DesktopNotifier.Converters;

public sealed class BoolToStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isRunning = value is true;
        return isRunning ? "laufend" : "gestoppt";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
