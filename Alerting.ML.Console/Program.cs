using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using Alerting.ML.Sources.Azure;
using Alerting.ML.TimeSeries.Sample;

var optimizationConfiguration = OptimizationConfiguration.Default;

var knownOutagesProvider = new SampleOutagesProvider();
var geneticOptimizer = new GeneticOptimizerStateMachine<ScheduledQueryRuleConfiguration>(new ScheduledQueryRuleAlert(),
    new SampleTimeSeriesProvider(knownOutagesProvider), knownOutagesProvider, new DefaultAlertScoreCalculator(),
    new DefaultConfigurationFactory<ScheduledQueryRuleConfiguration>(), new NullEventStore(), optimizationConfiguration);

var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));

await foreach (var alertScoreCard in geneticOptimizer.Optimize(cancellationTokenSource.Token))
{
    Console.WriteLine($"Score: {alertScoreCard.Best.Score}. {alertScoreCard}");
}

public class NullEventStore : IEventStore
{
    public async Task Write<T>(Guid aggregateId, T @event) where T: IEvent
    {
        Console.WriteLine($"{aggregateId} :: {@event}");
    }

    public async IAsyncEnumerable<IEvent> GetAll(Guid aggregateId)
    {
        yield break;
    }
}