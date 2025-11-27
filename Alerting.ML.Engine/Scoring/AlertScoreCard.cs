using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Engine.Scoring;

public sealed class AlertScoreCard
{
    public double Score { get; }

    public AlertScoreCard(double precision, TimeSpan medianDetectionLatency, double falseNegativeRate, int outagesCount,
        IAlertConfiguration configuration, AlertScoreConfiguration scoreConfiguration)
    {
        Precision = precision;
        MedianDetectionLatency = medianDetectionLatency;
        FalseNegativeRate = falseNegativeRate;
        Configuration = configuration;
        OutagesCount = outagesCount;

        var precisionDelta = Math.Max(scoreConfiguration.PrecisionTarget - Precision, 0);
        var latencyDelta = Math.Abs(MedianDetectionLatency.TotalMinutes) < scoreConfiguration.MedianDetectionLatencyTarget.TotalMinutes ? 0 : scoreConfiguration.MedianDetectionLatencyTarget.TotalMinutes - MedianDetectionLatency.TotalMinutes;

        Score = ComputeScore(precisionDelta, latencyDelta, FalseNegativeRate);
        IsNotFeasible = outagesCount == 0;
    }

    private static double ComputeScore(double precisionDelta, double latencyDelta, double falseNegativeRate)
    {
        return Math.Sqrt(Math.Pow(precisionDelta * 100, 2) +
                         Math.Pow(latencyDelta, 2) +
                         Math.Pow(falseNegativeRate * 100, 2));
    }

    public bool IsNotFeasible { get; }

    public double Precision { get; }
    public TimeSpan MedianDetectionLatency { get; }
    public double FalseNegativeRate { get; }
    public IAlertConfiguration Configuration { get; }
    public int OutagesCount { get; }

    public override string ToString()
    {
        return
            $"{nameof(Precision)}: {Precision:P}, {nameof(MedianDetectionLatency)}: {MedianDetectionLatency:g}, {nameof(FalseNegativeRate)}: {FalseNegativeRate:P}";
    }
}