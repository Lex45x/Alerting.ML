using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

internal class SurvivorsCountedEvent<T> : IEvent where T : AlertConfiguration
{
    public IReadOnlyList<T> Survivors { get; }

    public SurvivorsCountedEvent(IReadOnlyList<T> survivors)
    {
        Survivors = survivors;
    }

    public override string ToString()
    {
        return $"SurvivorsCountedEvent: SurvivorsCount: {Survivors.Count}";
    }
}