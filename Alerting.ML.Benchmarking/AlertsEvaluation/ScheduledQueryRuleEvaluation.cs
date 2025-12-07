using Alerting.ML.Engine.Data;
using Alerting.ML.Sources.Azure;
using Alerting.ML.TimeSeries.Sample;
using BenchmarkDotNet.Attributes;

namespace Alerting.ML.Benchmarking.AlertsEvaluation;

public class ScheduledQueryRuleEvaluation
{
    [GlobalSetup]
    public void InitializeAlert()
    {
        var outagesProvider = new SampleOutagesProvider();
        sampleTimeSeriesProvider = new SampleTimeSeriesProvider(outagesProvider);
        alert = new ScheduledQueryRuleAlert();
    }

    private ScheduledQueryRuleAlert alert;
    private SampleTimeSeriesProvider sampleTimeSeriesProvider;

    [Benchmark(Baseline = true)]
    public List<Outage> Evaluate()
    {
        var configuration = new ScheduledQueryRuleConfiguration
        {
            NumberOfEvaluationPeriods = 10,
            MinFailingPeriodsToAlert = 1,
            Operator = Operator.LessThan,
            Threshold = 96,
            EvaluationFrequency = WindowSizeAndFrequency.EvaluationFrequency,
            WindowSize = WindowSizeAndFrequency.WindowSize,
            TimeAggregation = TimeAggregation
        };

        var outages = alert.Evaluate(sampleTimeSeriesProvider.GetTimeSeries(), configuration).ToList();

        return outages;
    }

    [Benchmark]
    public List<Outage> EvaluateOptimized()
    {
        var configuration = new ScheduledQueryRuleConfiguration
        {
            NumberOfEvaluationPeriods = 10,
            MinFailingPeriodsToAlert = 1,
            Operator = Operator.LessThan,
            Threshold = 96,
            EvaluationFrequency = WindowSizeAndFrequency.EvaluationFrequency,
            WindowSize = WindowSizeAndFrequency.WindowSize,
            TimeAggregation = TimeAggregation
        };

        var outages = alert.EvaluateOptimized(sampleTimeSeriesProvider.GetTimeSeries(), configuration).ToList();

        return outages;
    }

    [Params(TimeAggregation.Count, TimeAggregation.Average, TimeAggregation.Minimum, TimeAggregation.Maximum, TimeAggregation.Total)]
    public TimeAggregation TimeAggregation { get; set; }

    [ParamsSource(nameof(WindowSizeAndFrequencySource))]
    public (TimeSpan WindowSize, TimeSpan EvaluationFrequency) WindowSizeAndFrequency { get; set; }

    public static IEnumerable<(TimeSpan WindowSize, TimeSpan EvaluationFrequency)> WindowSizeAndFrequencySource()
    {
        yield return (TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(1));
        yield return (TimeSpan.FromHours(1), TimeSpan.FromMinutes(1));
        yield return (TimeSpan.FromHours(2), TimeSpan.FromHours(1));
    }
}