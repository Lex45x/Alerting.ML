using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Alerting.ML.App.Converters.CloudProvider;

public class CloudProviderToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (value as Model.Enums.CloudProvider?) switch
        {
            Model.Enums.CloudProvider.Azure => "../../Assets/azure-icon.svg",
            Model.Enums.CloudProvider.Amazon => "../../Assets/aws-icon.svg",
            Model.Enums.CloudProvider.Google => "../../Assets/gcp-icon.svg",
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}