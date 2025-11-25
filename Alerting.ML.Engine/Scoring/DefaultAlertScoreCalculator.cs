using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Scoring;

public class DefaultAlertScoreCalculator : IAlertScoreCalculator
{
    public AlertScoreCard CalculateScore(IEnumerable<Outage> alertOutages, IKnownOutagesProvider knownOutagesProvider,
        IAlertConfiguration configuration, AlertScoreConfiguration scoreConfiguration)
    {
        var knownOutages = knownOutagesProvider.GetKnownOutages();
        var latencies = new List<TimeSpan>();
        var truePositiveCount = 0;
        int? totalCount = null;
        var detectedOutages = new HashSet<Outage>();


        foreach (var alertOutage in alertOutages)
        {
            totalCount ??= 0;
            totalCount++;
            var matchingOutage = knownOutages.SingleOrDefault(outage =>
                TimeSpan.FromTicks(Math.Abs((outage.StartTime - alertOutage.StartTime).Ticks)) <
                TimeSpan.FromHours(1));

            if (matchingOutage == null)
            {
                continue;
            }

            var detectionLatency = matchingOutage.StartTime - alertOutage.StartTime;
            latencies.Add(detectionLatency);
            truePositiveCount++;
            detectedOutages.Add(matchingOutage);
        }

        latencies.Sort();

        TimeSpan median;

        if (latencies.Count > 0)
        {
            if (latencies.Count % 2 == 1)
            {
                median = latencies[latencies.Count / 2];
            }
            else
            {
                median = (latencies[latencies.Count / 2 - 1] +
                          latencies[latencies.Count / 2] / 2);
            }
        }
        else
        {
            median = TimeSpan.FromDays(1);
        }


        return new AlertScoreCard(((double)truePositiveCount) / (totalCount ?? 1), median,
            ((double)knownOutages.Count - detectedOutages.Count) / knownOutages.Count, totalCount ?? 0, configuration, scoreConfiguration);
    }
}