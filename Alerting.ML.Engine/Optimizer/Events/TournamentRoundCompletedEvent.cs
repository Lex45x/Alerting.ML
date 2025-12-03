using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
/// Indicates a completion of the tournament round. <br/>
/// Contains two winning configurations of the given round.
/// </summary>
/// <param name="FirstWinner">First winner is guaranteed to be added into next generation.</param>
/// <param name="SecondWinner">Second winner will be added to next generation only if there is some room left.</param>
/// <param name="AggregateVersion">Version of the aggregate current event is applied.</param>
/// <typeparam name="T">Current alert configuration type</typeparam>
public record TournamentRoundCompletedEvent<T>(T FirstWinner, T SecondWinner, int AggregateVersion)
    : IEvent where T : AlertConfiguration;