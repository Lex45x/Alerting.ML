using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Alert;

/// <summary>
///     Represent an anomaly detection mechanism of a third-party service.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAlert<in T> : IAlert where T : AlertConfiguration
{
    /// <summary>
    ///     UI-friendly name of the alert provider.
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    ///     Takes a time-series from <paramref name="provider" /> and runs anomaly detection with given
    ///     <paramref name="configuration" />.
    ///     Produces a list of <see cref="Outage" />s that represent a set of time-windows when this alert would fire.
    /// </summary>
    /// <param name="provider">A source of time-series.</param>
    /// <param name="configuration">Alert configuration</param>
    /// <returns></returns>
    IEnumerable<Outage> Evaluate(ITimeSeriesProvider provider, T configuration);
}

/// <summary>
///     Non-generic version of the interface for a limited type-safety outside Generic context.
/// </summary>
public interface IAlert
{
}