using Alerting.ML.Engine.Storage;
using FluentValidation.Results;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
/// Indicates that training session experienced an unrecoverable failure.
/// </summary>
/// <param name="AggregateVersion"></param>
public record CriticalFailureEvent(int AggregateVersion, ValidationResult FailureDetails) :IEvent
{
    
}