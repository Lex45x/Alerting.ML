using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

internal class SummaryCreatedEvent : IEvent
{
    public SummaryCreatedEvent(GenerationSummary summary)
    {
        Summary = summary;
    }

    public GenerationSummary Summary { get; }
}