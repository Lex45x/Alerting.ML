namespace Alerting.ML.Engine.Scoring;

public record AlertScoreConfiguration(double PrecisionTarget, TimeSpan MedianDetectionLatencyTarget,
    AlertScorePriority Priority);