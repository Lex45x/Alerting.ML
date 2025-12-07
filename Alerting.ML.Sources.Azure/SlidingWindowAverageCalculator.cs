namespace Alerting.ML.Sources.Azure;

internal class SlidingWindowAverageCalculator : SlidingWindowTotalCalculator
{
    public override double Value => Total / Count;
}