using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Extensions;

namespace Alerting.ML.Sources.Azure;

internal class SlidingWindowMaxCalculator : ISlidingWindowCalculator
{
    public double Maximum { get; private set; }

    public void Add(double value, ReadOnlySpan<Metric> timeWindow)
    {
        Maximum = value >= Maximum ? value : Maximum;
    }

    public void Remove(double value, ReadOnlySpan<Metric> timeWindow)
    {
        Maximum = value >= Maximum ? timeWindow.Max(metric => metric.Value) : Maximum;
    }

    public double Value => Maximum;
}