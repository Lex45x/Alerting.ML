using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Sources.Azure;

internal class ThresholdParameterAttribute : ConfigurationParameterAttribute
{
    /// <inheritdoc />
    public override object GetRandomValue(AlertConfiguration appliedTo, TimeSeriesStatistics statistics)
    {
        return Random.Shared.Next((int)(statistics.Minimum - 1), (int)(statistics.Maximum + 1));
    }

    /// <inheritdoc />
    public override object Nudge(object value, AlertConfiguration appliedTo, TimeSeriesStatistics statistics)
    {
        var newValue = (int)value + Random.Shared.Next(-1, 1);
        return Math.Min(Math.Max(newValue, (int)statistics.Minimum), (int)statistics.Maximum);
    }
}