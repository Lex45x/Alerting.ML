using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Azure;

internal class SlidingWindowCountCalculator : ISlidingWindowCalculator
{
    public double Count { get; private set; }
    public virtual double Value => Count;

    public virtual void Add(double value, ReadOnlySpan<Metric> timeWindow)
    {
        Count += 1;
    }

    public virtual void Remove(double value, ReadOnlySpan<Metric> timeWindow)
    {
        Count -= 1;
    }
}