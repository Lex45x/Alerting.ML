using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public class TournamentRoundCompletedEvent<T> : IEvent where T : AlertConfiguration
{
    public T FirstWinner { get; }
    public T SecondWinner { get; }

    public TournamentRoundCompletedEvent(T firstWinner, T secondWinner)
    {
        FirstWinner = firstWinner;
        SecondWinner = secondWinner;
    }

    public override string ToString()
    {
        return $"TournamentRoundCompletedEvent: {nameof(FirstWinner)}: {FirstWinner}, {nameof(SecondWinner)}: {SecondWinner}";
    }
}