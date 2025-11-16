using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Scoring;

public interface IAlertScoreCalculator
{
    public AlertScoreCard CalculateScore(IEnumerable<Outage> alertOutages, IKnownOutagesProvider knownOutagesProvider,
        IAlertConfiguration configuration, AlertScoreConfiguration scoreConfiguration);
}