using Alerting.ML.Engine.Data;

namespace Alerting.ML.TimeSeries.Sample;

public class SampleOutagesProvider : IKnownOutagesProvider
{
    private static readonly DateTime Current = DateTime.UtcNow;

    private static readonly IReadOnlyList<Outage> Outages = Enumerable.Range(0, 30).Select(i =>
    {
        var startTime = Current.AddDays(Random.Shared.Next(0, 10) + i * 20);
        return new Outage(startTime, startTime.AddHours(6));
    }).ToList();

    public IReadOnlyList<Outage> GetKnownOutages()
    {
        return Outages;
    }
}