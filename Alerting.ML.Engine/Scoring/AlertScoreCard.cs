using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Engine.Scoring;

public sealed class AlertScoreCard
{
    public double Score { get; }

    private const double IdealPrecision = 1;
    private const double IdealLatencyMinutes = 0;

    public AlertScoreCard(double precision, TimeSpan medianDetectionLatency, double falseNegativeRate, int outagesCount,
        AlertConfiguration configuration, bool isNotFeasible)
    {
        Precision = precision;
        MedianDetectionLatency = medianDetectionLatency;
        FalseNegativeRate = falseNegativeRate;
        OutagesCount = outagesCount;
        Configuration = configuration;
        IsNotFeasible = isNotFeasible;

        var precisionDelta = Math.Max(IdealPrecision - Precision, 0);
        var latencyDelta = Math.Abs(IdealLatencyMinutes - MedianDetectionLatency.TotalMinutes);

        Score = ComputeScore(precisionDelta, latencyDelta, FalseNegativeRate);
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
    public AlertConfiguration Configuration { get; }
    public int OutagesCount { get; }

    public override string ToString()
    {
        return
            $"{nameof(Precision)}: {Precision:P}, {nameof(MedianDetectionLatency)}: {MedianDetectionLatency:g}, {nameof(FalseNegativeRate)}: {FalseNegativeRate:P}";
    }
}