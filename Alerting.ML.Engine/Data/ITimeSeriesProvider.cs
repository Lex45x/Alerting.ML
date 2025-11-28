using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Engine.Data;

/// <summary>
/// Provides a metric data for <see cref="IAlert{T}"/> evaluation.
/// </summary>
public interface ITimeSeriesProvider
{
    /// <summary>
    /// Returns an ordered (earliest to latest) array of metric values.
    /// todo: returned value must be immutable
    /// </summary>
    /// <returns></returns>
    Metric[] GetTimeSeries();
}