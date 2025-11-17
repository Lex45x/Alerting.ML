using Avalonia.Media;
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Alerting.ML.App.Converters.ProgressBar;

public class ProgressBarColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is double v)
        {
            if (v >= 0.8) return new SolidColorBrush(Color.Parse("#22C55E"));
            if (v >= 0.5) return new SolidColorBrush(Color.Parse("#3B82F6"));
            return new SolidColorBrush(Color.Parse("#F97316"));
        }

        return Brushes.Gray;
    }

    public object ConvertBack(object v, Type t, object p, CultureInfo c)
        => throw new NotImplementedException();
}