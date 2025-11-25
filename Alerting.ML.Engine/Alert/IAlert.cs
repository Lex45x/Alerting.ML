using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Alert;

public interface IAlert<in T> : IAlert where T : AlertConfiguration<T>
{
    IEnumerable<Outage> Evaluate(ITimeSeriesProvider provider, T configuration);
    IEnumerable<Outage> IAlert.Evaluate(ITimeSeriesProvider provider, object configuration)
    {
        return Evaluate(provider, configuration as T ?? throw new ArgumentException($"Unsupported configuration type {configuration.GetType()}. {typeof(T)} is expected.", paramName: nameof(configuration)));
    }
}

public interface IAlert
{
    IEnumerable<Outage> Evaluate(ITimeSeriesProvider provider, object configuration);
}