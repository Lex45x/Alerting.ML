namespace Alerting.ML.Engine.Data;

/// <summary>
///     A single point of a time-series.
/// </summary>
public readonly struct Metric : IEquatable<Metric>
{
    /// <summary>
    ///     Creates a new instance of Metric with a time point and a metric value
    /// </summary>
    /// <param name="timestamp"></param>
    /// <param name="value"></param>
    public Metric(DateTime timestamp, double value)
    {
        Timestamp = timestamp;
        Value = value;
    }

    /// <summary>
    ///     Time when metric had a selected value.
    /// </summary>
    public DateTime Timestamp { get; }

    /// <summary>
    ///     Value of the metric.
    /// </summary>
    public double Value { get; }

    /// <inheritdoc />
    public bool Equals(Metric other)
    {
        return Timestamp.Equals(other.Timestamp) && Value.Equals(other.Value);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return obj is Metric other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Timestamp, Value);
    }
}