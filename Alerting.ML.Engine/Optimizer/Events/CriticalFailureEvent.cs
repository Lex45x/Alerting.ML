using Alerting.ML.Engine.Storage;
using FluentValidation.Results;

namespace Alerting.ML.Engine.Optimizer.Events;

/// <summary>
///     Indicates that training session experienced an unrecoverable failure.
/// </summary>
/// <param name="AggregateVersion">Version of the aggregate at which failure happened.</param>
/// <param name="FailureDetails">Description of the failure.</param>
public record CriticalFailureEvent(int AggregateVersion, ValidationResult FailureDetails) : IEvent
{
    /// <summary>
    /// Creates critical failure event from the exception <paramref name="e"/> and preserves <see cref="Exception.Message"/> in validation errors.
    /// </summary>
    /// <param name="aggregateVersion">Version of the aggregate where this failure happened.</param>
    /// <param name="e">Exception that caused failure.</param>
    /// <returns>New instance of critical failure event.</returns>
    public static CriticalFailureEvent FromException(int aggregateVersion, Exception e)
    {
        return new CriticalFailureEvent(aggregateVersion, new ValidationResult
        {
            Errors =
            [
                new ValidationFailure
                {
                    ErrorMessage = e.Message
                }
            ]
        });
    }
}