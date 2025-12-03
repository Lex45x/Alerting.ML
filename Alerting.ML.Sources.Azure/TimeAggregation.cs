namespace Alerting.ML.Sources.Azure;

/// <summary>
///     Defines an aggregation function to be applied to a specific time-window of metric values.
/// </summary>
public enum TimeAggregation
{
    // Self-explanatory enum members.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    Average,
    Minimum,
    Maximum,
    Total,
    Count
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}