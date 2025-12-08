using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Extensions;

namespace Alerting.ML.Sources.Azure;

internal class SlidingWindowMinCalculator : ISlidingWindowCalculator
{
    public double Minimum { get; private set; }

    public void Add(double value, ReadOnlySpan<Metric> timeWindow)
    {
        Minimum = value <= Minimum ? value : Minimum;
    }

    public void Remove(double value, ReadOnlySpan<Metric> timeWindow)
    {
        Minimum = value <= Minimum ? timeWindow.Min(metric => metric.Value) : Minimum;
    }

    public double Value => Minimum;
}