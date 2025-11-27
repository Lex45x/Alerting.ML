using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

internal class EvaluationCompletedEvent<T> : IEvent where T : AlertConfiguration<T>
{
    public T Configuration { get; }
    public IReadOnlyList<Outage> Outages { get; }

    public EvaluationCompletedEvent(T configuration, IReadOnlyList<Outage> outages)
    {
        Configuration = configuration;
        Outages = outages;
    }

    public override string ToString()
    {
        return $"EvaluationCompletedEvent: {nameof(Configuration)}: {Configuration}, {nameof(Outages)}: {Outages.Count}";
    }
}