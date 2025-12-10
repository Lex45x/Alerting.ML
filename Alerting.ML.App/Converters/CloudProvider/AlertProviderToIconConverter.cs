using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Alerting.ML.App.Converters.CloudProvider;

public class AlertProviderToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as Model.Enums.CloudProvider?) switch
        {
            Model.Enums.CloudProvider.Azure => "avares://Alerting.ML.App/Assets/azure-icon.svg",
            Model.Enums.CloudProvider.Amazon => "avares://Alerting.ML.App/Assets/aws-icon.svg",
            Model.Enums.CloudProvider.Google => "avares://Alerting.ML.App/Assets/gcp-icon.svg",
            _ => "avares://Alerting.ML.App/Assets/cloud-icon.svg"
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}