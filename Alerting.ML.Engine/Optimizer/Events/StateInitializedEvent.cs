using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record StateInitializedEvent<T> : IEvent where T : AlertConfiguration
{
    public IAlert<T> Alert { get; }
    public ITimeSeriesProvider TimeSeriesProvider { get; }
    public IKnownOutagesProvider KnownOutagesProvider { get; }
    public IAlertScoreCalculator AlertScoreCalculator { get; }
    public IConfigurationFactory<T> ConfigurationFactory { get; }
    public Guid Id { get; }
    public DateTime CreatedAt { get; }
    public string Name { get; }
    public string ProviderName { get; }

    public StateInitializedEvent(Guid id, DateTime createdAt, string name, string providerName, IAlert<T> alert, ITimeSeriesProvider timeSeriesProvider,
        IKnownOutagesProvider knownOutagesProvider, IAlertScoreCalculator alertScoreCalculator,
        IConfigurationFactory<T> configurationFactory, int aggregateVersion)
    {
        Alert = alert;
        TimeSeriesProvider = timeSeriesProvider;
        KnownOutagesProvider = knownOutagesProvider;
        AlertScoreCalculator = alertScoreCalculator;
        ConfigurationFactory = configurationFactory;
        AggregateVersion = aggregateVersion;
        Id = id;
        CreatedAt = createdAt;
        Name = name;
        ProviderName = providerName;
    }

    public int AggregateVersion { get; }
}