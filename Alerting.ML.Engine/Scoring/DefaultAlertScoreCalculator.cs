using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Scoring;

/// <summary>
///     Creates a <see cref="AlertScoreCard" /> based on detected outages comparison to known outages.
/// </summary>
public class DefaultAlertScoreCalculator : IAlertScoreCalculator
{
    private const double OnePercent = 0.01;


    /// <inheritdoc />
    public AlertScoreCard CalculateScore(IEnumerable<Outage> alertOutages, IKnownOutagesProvider knownOutagesProvider,
        AlertConfiguration configuration)
    {
        var knownOutages = knownOutagesProvider.GetKnownOutages();
        var latencies = new List<TimeSpan>();
        var truePositiveCount = 0;
        var totalCount = 0;
        var detectedOutages = new HashSet<Outage>();


        foreach (var alertOutage in alertOutages)
        {
            totalCount++;
            var matchingOutage = knownOutages.SingleOrDefault(outage =>
                TimeSpan.FromTicks(Math.Abs((outage.StartTime - alertOutage.StartTime).Ticks)) <
                TimeSpan.FromHours(hours: 1));

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
                median = latencies[latencies.Count / 2 - 1] +
                         latencies[latencies.Count / 2] / 2;
            }
        }
        else
        {
            median = TimeSpan.FromDays(days: 1);
        }

        var precision = (double)truePositiveCount / Math.Max(totalCount, val2: 1);
        var falseNegativeRate = ((double)knownOutages.Count - detectedOutages.Count) / knownOutages.Count;
        var isNotFeasible = totalCount == 0 || precision < OnePercent;

        return new AlertScoreCard(precision, median,
            falseNegativeRate, totalCount, configuration, isNotFeasible);
    }
}