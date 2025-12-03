using System;
using System.Globalization;
using Alerting.ML.Sources.Azure;
using Alerting.ML.Sources.Csv;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Alerting.ML.App.Converters.TrainingBuilder;

public class TrainingBuilderToPreviewTableItemConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        //todo: build a reflection based automatic assembly scan. Design smaller components that can be wired to a specific type via interfaces smth like: IPreviewPresenterFor<CsvTimeSeriesProvider>
        return value switch
        {
            CsvTimeSeriesProvider provider => new TextBlock
            {
                Text = $"CSV File: {provider.FileName}",
                Classes = { "h4" }
            },
            ScheduledQueryRuleAlert alert => new TextBlock
            {
                Text = "Azure Scheduled Query Rule Alert",
                Classes = { "h4" }
            },
            CsvOutagesProvider provider => new TextBlock
            {
                Text = provider.FileName,
                Classes = { "h4" }
            },
            string s => new TextBlock
            {
                Text = s,
                Classes = { "h4" }
            },
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, message: null)
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}