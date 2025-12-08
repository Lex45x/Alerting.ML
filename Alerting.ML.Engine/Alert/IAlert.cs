using System.Collections.Immutable;
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
    ///     Takes <paramref name="timeSeries" /> and runs anomaly detection with given
    ///     <paramref name="configuration" />.
    ///     Produces a list of <see cref="Outage" />s that represent a set of time-windows when this alert would fire.
    ///     Evaluations must be thread-safe.
    /// </summary>
    /// <param name="timeSeries"></param>
    /// <param name="configuration">Alert configuration</param>
    /// <returns></returns>
    IEnumerable<Outage> Evaluate(ImmutableArray<Metric> timeSeries, T configuration);
}

/// <summary>
///     Non-generic version of the interface for a limited type-safety outside Generic context.
/// </summary>
public interface IAlert
{
}