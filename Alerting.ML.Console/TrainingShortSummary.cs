using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Console;

public class TrainingShortSummary
{
    public override string ToString()
    {
        return $"{Id} {Name} {Status}";
    }

    public virtual void Apply(IEvent @event)
    {
        switch (@event)
        {
            case StateInitializedEvent initialized:
                Id = initialized.Id;
                Name = initialized.Name;
                break;
            case TrainingCompletedEvent:
                Status = Status.Completed;
                break;
            case CriticalFailureEvent:
                Status = Status.Failed;
                break;
        }
    }

    public Status Status { get; private set; } = Status.Training;

    public string Name { get; private set; }

    public Guid Id { get; private set; }
}