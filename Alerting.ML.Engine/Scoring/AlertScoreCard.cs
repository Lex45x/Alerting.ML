using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Engine.Scoring;

/// <summary>
/// Represents a performance score for <see cref="AlertConfiguration"/> in the context of ongoing optimization.
/// </summary>
public sealed class AlertScoreCard
{
    /// <summary>
    /// A value from 0 to infinity that represents a multidimensional 'distance' from ideal alert performance.
    /// Lower value is better. 
    /// </summary>
    public double Score { get; }

    private const double IdealPrecision = 1;
    private const double IdealLatencyMinutes = 0;

    /// <summary>
    /// Creates a scorecard.
    /// </summary>
    /// <param name="precision">Determines a precision of alert with a given configuration.</param>
    /// <param name="medianDetectionLatency">Median delay between alert firing and a know outage occuring.</param>
    /// <param name="falseNegativeRate">Percentage of incidents that were not correlated with any known outage.</param>
    /// <param name="outagesCount">Total amount of outages created by alert.</param>
    /// <param name="configuration">A configuration that resulted in a given score.</param>
    /// <param name="isNotFeasible">Indicates that given configuration did not produce any meaningful results and should not be used further.</param>
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

    /// <summary>
    /// Computes pythagorean distance from ideal performance according to given parameters
    /// </summary>
    private static double ComputeScore(double precisionDelta, double latencyDelta, double falseNegativeRate)
    {
        return Math.Sqrt(Math.Pow(precisionDelta * 100, 2) +
                         Math.Pow(latencyDelta, 2) +
                         Math.Pow(falseNegativeRate * 100, 2));
    }

    /// <summary>
    /// Indicates that given configuration did not produce any meaningful results and should not be used further.
    /// </summary>
    public bool IsNotFeasible { get; }

    /// <summary>
    /// Determines a precision of alert with a given configuration.
    /// </summary>
    public double Precision { get; }

    /// <summary>
    /// Median delay between alert firing and a know outage occuring.
    /// </summary>
    public TimeSpan MedianDetectionLatency { get; }

    /// <summary>
    /// Percentage of incidents that were not correlated with any known outage.
    /// </summary>
    public double FalseNegativeRate { get; }

    /// <summary>
    /// A configuration that resulted in a given score.
    /// </summary>
    public AlertConfiguration Configuration { get; }

    /// <summary>
    /// Total amount of outages created by alert.
    /// </summary>
    public int OutagesCount { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{nameof(Score)}: {Score:N}, {nameof(OutagesCount)}: {OutagesCount}, " +
               $"{nameof(IsNotFeasible)}: {IsNotFeasible}, {nameof(Precision)}: {Precision:P}, " +
               $"{nameof(MedianDetectionLatency)}: {MedianDetectionLatency:g}, {nameof(FalseNegativeRate)}: {FalseNegativeRate:P}";
    }
}