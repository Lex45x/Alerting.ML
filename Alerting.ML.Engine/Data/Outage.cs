namespace Alerting.ML.Engine.Data;

/// <summary>
/// Represents an anomaly that was happening between <paramref name="StartTime"/> and <paramref name="EndTime"/>
/// </summary>
/// <param name="StartTime">Start of an outage.</param>
/// <param name="EndTime">End of the outage.</param>
public record Outage(DateTime StartTime, DateTime EndTime);