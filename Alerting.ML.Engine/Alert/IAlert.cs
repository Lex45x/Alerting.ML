using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Alert;

public interface IAlert<in T> where T : AlertConfiguration<T>
{
    IEnumerable<Outage> Evaluate(ITimeSeriesProvider provider, T configuration);
}