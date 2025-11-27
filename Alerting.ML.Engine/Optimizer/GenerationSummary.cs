using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.Engine.Optimizer;

public class GenerationSummary
{
    public int GenerationIndex { get; }

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

    public IReadOnlyList<(int Count, double Value)> ScoreDistribution { get; }

    public IReadOnlyList<(int Count, TimeSpan Value)> DetectionLatencyDistribution { get; }

    public IReadOnlyList<(int Count, double Value)> FalseNegativeRateDistribution { get; }

    public IReadOnlyList<(int Count, double Value)> PrecisionDistribution { get; }
    public IReadOnlyList<(int Count, double Value)> OutageCountDistribution { get; }

    public AlertScoreCard Best { get; }
}