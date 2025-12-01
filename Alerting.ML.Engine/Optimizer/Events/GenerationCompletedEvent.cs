using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer.Events;

public class GenerationCompletedEvent : IEvent
{
    public GenerationCompletedEvent(GenerationSummary summary)
    {
        Summary = summary;
    }

    public GenerationSummary Summary { get; }
}