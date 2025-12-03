using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Scoring;

/// <summary>
///     Evaluates alert and produces <see cref="AlertScoreCard" />
/// </summary>
public interface IAlertScoreCalculator
{
    /// <summary>
    ///     Creates a scorecard based on the alert performance.
    /// </summary>
    /// <param name="alertOutages">
    ///     Outages detected by <see cref="IAlert{T}" /> with configuration
    ///     <paramref name="configuration" />
    /// </param>
    /// <param name="knownOutagesProvider">A set of known outages associated with that time-series.</param>
    /// <param name="configuration">A configuration that was used by <see cref="IAlert{T}" /></param>
    /// <returns>Score evaluation of the alert.</returns>
    public AlertScoreCard CalculateScore(IEnumerable<Outage> alertOutages, IKnownOutagesProvider knownOutagesProvider,
        AlertConfiguration configuration);
}