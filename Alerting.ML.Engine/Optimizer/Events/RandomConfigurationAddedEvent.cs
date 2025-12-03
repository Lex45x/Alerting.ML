using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record RandomConfigurationAddedEvent<T> : IEvent where T : AlertConfiguration
{

    public T RandomConfiguration { get; }

    public RandomConfigurationAddedEvent(T randomConfiguration, int aggregateVersion)
    {
        RandomConfiguration = randomConfiguration;
        AggregateVersion = aggregateVersion;
    }

    public int AggregateVersion { get; }
}