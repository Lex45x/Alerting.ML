using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Azure;

internal interface ISlidingWindowCalculator
{
    public void Add(double value, ReadOnlySpan<Metric> timeWindow);
    public void Remove(double value, ReadOnlySpan<Metric> timeWindow);
    public double Value { get; }
}