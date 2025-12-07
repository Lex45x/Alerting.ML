using System.Collections.Immutable;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
///     The first event that initializes <see cref="GeneticOptimizerState{T}" /> <br />
/// </summary>
/// <param name="Id">Id of the optimization session.</param>
/// <param name="CreatedAt">DateTime of session creation.</param>
/// <param name="Name">Friendly name of the session.</param>
/// <param name="ProviderName">FriendlyName of alert provider.</param>
/// <param name="Alert">Alert rule.</param>
/// <param name="TimeSeries">Time series.</param>
/// <param name="KnownOutages">Known outages.</param>
/// <param name="AlertScoreCalculator">Score calculator.</param>
/// <param name="ConfigurationFactory">Configuration Factory</param>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
/// <typeparam name="T">Current alert configuration type.</typeparam>
public record StateInitializedEvent<T>(
    Guid Id,
    DateTime CreatedAt,
    string Name,
    string ProviderName,
    IAlert<T> Alert,
    ImmutableArray<Metric> TimeSeries,
    IReadOnlyList<Outage> KnownOutages,
    IAlertScoreCalculator AlertScoreCalculator,
    IConfigurationFactory<T> ConfigurationFactory,
    int AggregateVersion)
    : IEvent
    where T : AlertConfiguration;