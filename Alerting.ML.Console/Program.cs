using Alerting.ML.Console;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using Alerting.ML.Sources.Azure;
using Alerting.ML.TimeSeries.Sample;

var knownOutagesProvider = new SampleOutagesProvider();
var sampleTimeSeriesProvider = new SampleTimeSeriesProvider(knownOutagesProvider);


await using var fileStream = File.OpenWrite("outages_1.csv");
using var writer = new StreamWriter(fileStream);

foreach (var knownOutage in knownOutagesProvider.GetKnownOutages())
{
    writer.WriteLine($"{knownOutage.StartTime},{knownOutage.EndTime}");
}

await using var seriesStream = File.OpenWrite("timeseries_1.csv");
using var seriesWriter = new StreamWriter(seriesStream);

foreach (var metric in sampleTimeSeriesProvider.GetTimeSeries())
{
    seriesWriter.WriteLine($"{metric.Timestamp},{metric.Value}");
}


//var geneticOptimizer = new GeneticOptimizerStateMachine<ScheduledQueryRuleConfiguration>(new ScheduledQueryRuleAlert(),
//    sampleTimeSeriesProvider, knownOutagesProvider, new DefaultAlertScoreCalculator(),
//    new DefaultConfigurationFactory<ScheduledQueryRuleConfiguration>(), new NullEventStore());

//AlertScoreCard? generationBest = null;

//await foreach (var @event in geneticOptimizer.Optimize(OptimizationConfiguration.Default, CancellationToken.None))
//{
//    switch (@event)
//    {
//        case AlertScoreComputedEvent alertScoreComputedEvent:
//            generationBest =
//                generationBest == null || generationBest.Score > alertScoreComputedEvent.AlertScoreCard.Score
//                    ? alertScoreComputedEvent.AlertScoreCard
//                    : generationBest;
//            break;
//        case GenerationCompletedEvent _:
//        {
//            if (generationBest != null)
//            {
//                Console.WriteLine($"Score: {generationBest.Score}. {generationBest}");
//            }

//            generationBest = null;
//            break;
//        }
//    }
//}

namespace Alerting.ML.Console
{
    public class NullEventStore : IEventStore
    {
        public async Task Write<T>(Guid aggregateId, T @event) where T : IEvent
        {
            System.Console.WriteLine($"{aggregateId} :: {@event}");
        }

        public async IAsyncEnumerable<IEvent> GetAll(Guid aggregateId)
        {
            yield break;
        }
    }
}