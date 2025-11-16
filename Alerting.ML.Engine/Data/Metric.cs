namespace Alerting.ML.Engine.Data;

public readonly struct Metric
{
    public Metric(DateTime timestamp, double value)
    {
        this.Timestamp = timestamp;
        this.Value = value;
    }

    public DateTime Timestamp { get; init; }
    public double Value { get; init; }

    public void Deconstruct(out DateTime Timestamp, out double Value)
    {
        Timestamp = this.Timestamp;
        Value = this.Value;
    }
}