using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

internal class RandomConfigurationAddedEvent<T> : IEvent where T : AlertConfiguration
{
    public override string ToString()
    {
        return $"RandomConfigurationAddedEvent: {nameof(RandomConfiguration)}: {RandomConfiguration}";
    }

    public T RandomConfiguration { get; }

    public RandomConfigurationAddedEvent(T randomConfiguration)
    {
        RandomConfiguration = randomConfiguration;
    }
}