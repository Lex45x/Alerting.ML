using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Engine.Scoring;

/// <summary>
///     Represents a performance score for <see cref="AlertConfiguration" /> in the context of ongoing optimization.
/// </summary>
public sealed class AlertScoreCard : IEquatable<AlertScoreCard>
{
    private const double IdealPrecision = 1;
    private const double IdealLatencyMinutes = 0;
    private const double NormalizationCoefficient = 0.001625;

    /// <summary>
    ///     Creates a scorecard.
    /// </summary>
    /// <param name="precision">Determines a precision of alert with a given configuration.</param>
    /// <param name="medianDetectionLatency">Median delay between alert firing and a know outage occuring.</param>
    /// <param name="recall">Percentage of undetected known outages.</param>
    /// <param name="outagesCount">Total amount of outages created by alert.</param>
    /// <param name="configuration">A configuration that resulted in a given score.</param>
    /// <param name="isNotFeasible">
    ///     Indicates that given configuration did not produce any meaningful results and should not be
    ///     used further.
    /// </param>
    public AlertScoreCard(double precision, TimeSpan medianDetectionLatency, double recall, int outagesCount,
        AlertConfiguration configuration, bool isNotFeasible)
    {
        Precision = precision;
        MedianDetectionLatency = medianDetectionLatency;
        Recall = recall;
        OutagesCount = outagesCount;
        Configuration = configuration;
        IsNotFeasible = isNotFeasible;

        var precisionDelta = Math.Max(IdealPrecision - Precision, val2: 0);
        var latencyDelta = Math.Abs(IdealLatencyMinutes - MedianDetectionLatency.TotalMinutes);

        Score = ComputeScore(precisionDelta, latencyDelta, Recall);
    }

    /// <summary>
    ///     A value from 0 to infinity that represents a Euclidean distance from ideal alert performance.
    ///     Lower value is better.
    /// </summary>
    public double Score { get; }

    /// <summary>
    ///     Normalized score value. Is between 0 and 1 where higher is better.
    /// </summary>
    public double Fitness => Math.Exp(-NormalizationCoefficient * Score);

    /// <summary>
    ///     Indicates that given configuration did not produce any meaningful results and should not be used further.
    /// </summary>
    public bool IsNotFeasible { get; }

    /// <summary>
    ///     Determines a precision of alert with a given configuration.
    /// </summary>
    public double Precision { get; }

    /// <summary>
    ///     Median delay between alert firing and a know outage occuring.
    /// </summary>
    public TimeSpan MedianDetectionLatency { get; }

    /// <summary>
    ///     Percentage of undetected known outages.
    /// </summary>
    public double Recall { get; }

    /// <summary>
    ///     A configuration that resulted in a given score.
    /// </summary>
    public AlertConfiguration Configuration { get; }

    /// <summary>
    ///     Total amount of outages created by alert.
    /// </summary>
    public int OutagesCount { get; }

    /// <inheritdoc />
    public bool Equals(AlertScoreCard? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Score.Equals(other.Score) && IsNotFeasible == other.IsNotFeasible && Precision.Equals(other.Precision) &&
               MedianDetectionLatency.Equals(other.MedianDetectionLatency) &&
               Recall.Equals(other.Recall) && Configuration.Equals(other.Configuration) &&
               OutagesCount == other.OutagesCount;
    }

    /// <summary>
    ///     Computes pythagorean distance from ideal performance according to given parameters
    /// </summary>
    private static double ComputeScore(double precisionDelta, double latencyDelta, double falseNegativeRate)
    {
        return Math.Sqrt(Math.Pow(precisionDelta * 100, y: 2) +
                         Math.Pow(latencyDelta, y: 2) +
                         Math.Pow(falseNegativeRate * 100, y: 2));
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(Score)}: {Score:N}, {nameof(OutagesCount)}: {OutagesCount}, " +
               $"{nameof(IsNotFeasible)}: {IsNotFeasible}, {nameof(Precision)}: {Precision:P}, " +
               $"{nameof(MedianDetectionLatency)}: {MedianDetectionLatency:g}, {nameof(Recall)}: {Recall:P}";
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || (obj is AlertScoreCard other && Equals(other));
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Score, IsNotFeasible, Precision, MedianDetectionLatency, Recall,
            Configuration, OutagesCount);
    }
}