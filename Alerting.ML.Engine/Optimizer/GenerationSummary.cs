using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// Provides statistics of the alert configurations in a single training generation.
/// </summary>
public class GenerationSummary
{
    /// <summary>
    /// Index of the generation.
    /// </summary>
    public int GenerationIndex { get; }

    /// <summary>
    /// Creates new instance of generation summary with <paramref name="generationIndex"/> and a list of <see cref="AlertScoreCard"/> from that generation.
    /// </summary>
    /// <param name="generationIndex"></param>
    /// <param name="generation"></param>
    public GenerationSummary(int generationIndex, IReadOnlyList<AlertScoreCard> generation)
    {
        GenerationIndex = generationIndex;
        Best = generation[0];
        PrecisionDistribution = generation
            .BinBy(card => card.Precision, 0.1, 2)
            .ToList();
        FalseNegativeRateDistribution = generation
            .BinBy(card => card.FalseNegativeRate, 0.1, 2)
            .ToList();
        DetectionLatencyDistribution = generation
            .BinBy(card => card.MedianDetectionLatency.Ticks, TimeSpan.FromMinutes(1).Ticks)
            .Select(tuple => (tuple.Count, TimeSpan.FromTicks((long)tuple.Value)))
            .ToList();
        ScoreDistribution = generation
            .BinBy(card => card.Score, 100)
            .ToList();
        OutageCountDistribution = generation
            .BinBy(card => card.OutagesCount, 1)
            .ToList();
    }

    /// <summary>
    /// Count of <see cref="AlertScoreCard"/> that falls into each <see cref="AlertScoreCard.Score"/> bucket.
    /// </summary>
    public IReadOnlyList<(int Count, double Value)> ScoreDistribution { get; }

    /// <summary>
    /// Count of <see cref="AlertScoreCard"/> that falls into each <see cref="AlertScoreCard.MedianDetectionLatency"/> bucket.
    /// </summary>
    public IReadOnlyList<(int Count, TimeSpan Value)> DetectionLatencyDistribution { get; }

    /// <summary>
    /// Count of <see cref="AlertScoreCard"/> that falls into each <see cref="AlertScoreCard.FalseNegativeRate"/> bucket.
    /// </summary>
    public IReadOnlyList<(int Count, double Value)> FalseNegativeRateDistribution { get; }
    /// <summary>
    /// Count of <see cref="AlertScoreCard"/> that falls into each <see cref="AlertScoreCard.Precision"/> bucket.
    /// </summary>
    public IReadOnlyList<(int Count, double Value)> PrecisionDistribution { get; }
    /// <summary>
    /// Count of <see cref="AlertScoreCard"/> that falls into each <see cref="AlertScoreCard.OutagesCount"/> bucket.
    /// </summary>
    public IReadOnlyList<(int Count, double Value)> OutageCountDistribution { get; }

    /// <summary>
    /// <see cref="AlertScoreCard"/> with the best <see cref="AlertScoreCard.Score"/> from generation <see cref="GenerationIndex"/>
    /// </summary>
    public AlertScoreCard Best { get; }
}