using Alerting.ML.Engine.Data;

namespace Alerting.ML.TimeSeries.Sample;

/// <summary>
/// Generates 30 random outages in last 70-ish days.
/// </summary>
public class SampleOutagesProvider : IKnownOutagesProvider
{
    private static readonly DateTime Current = DateTime.UtcNow;

    private static readonly IReadOnlyList<Outage> Outages = Enumerable.Range(0, 30).Select(i =>
    {
        var startTime = Current.AddDays(Random.Shared.Next(0, 10) + i * 20);
        return new Outage(startTime, startTime.AddHours(6));
    }).ToList();

    /// <inheritdoc />
    public IReadOnlyList<Outage> GetKnownOutages()
    {
        return Outages;
    }
}