using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Azure;

internal class SlidingWindowTotalCalculator : SlidingWindowCountCalculator
{
    public double Total { get; private set; }
    public override double Value => Total;

    public override void Add(double value, ReadOnlySpan<Metric> timeWindow)
    {
        base.Add(value, timeWindow);
        Total += value;
    }

    public override void Remove(double value, ReadOnlySpan<Metric> timeWindow)
    {
        base.Remove(value, timeWindow);
        Total -= value;
    }
}