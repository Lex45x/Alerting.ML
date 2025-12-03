using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public record TournamentRoundCompletedEvent<T> : IEvent where T : AlertConfiguration
{
    public T FirstWinner { get; }
    public T SecondWinner { get; }

    public TournamentRoundCompletedEvent(T firstWinner, T secondWinner, int aggregateVersion)
    {
        FirstWinner = firstWinner;
        SecondWinner = secondWinner;
        AggregateVersion = aggregateVersion;
    }

    public int AggregateVersion { get; }
}