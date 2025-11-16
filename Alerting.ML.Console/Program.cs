using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Sources.Azure;
using Alerting.ML.TimeSeries.Sample;using Microsoft.Extensions.Logging;

var loggerFactory = LoggerFactory.Create(builder => { });

var knownOutagesProvider = new SampleOutagesProvider();
var geneticOptimizer = new GeneticOptimizer<ScheduledQueryRuleConfiguration>(new ScheduledQueryRuleAlert(),
    new SampleTimeSeriesProvider(knownOutagesProvider), knownOutagesProvider, new DefaultAlertScoreCalculator(),
    new DefaultConfigurationFactory<ScheduledQueryRuleConfiguration>(), loggerFactory.CreateLogger<GeneticOptimizer<ScheduledQueryRuleConfiguration>>());

var alertScoreConfiguration = new AlertScoreConfiguration(0.9, TimeSpan.FromMinutes(5), AlertScorePriority.Precision);
var optimizationConfiguration = new OptimizationConfiguration(1_000, 0.01, 0.7, 100, alertScoreConfiguration, 3);

foreach (var alertScoreCard in geneticOptimizer.Optimize(optimizationConfiguration))
{
    Console.WriteLine($"Score: {alertScoreCard.Best.Score}. {alertScoreCard}");
}