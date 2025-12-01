using Alerting.ML.Engine.Alert;
using FluentValidation.Results;

namespace Alerting.ML.Engine.Data;

/// <summary>
/// <see cref="IAlert{T}"/> performance is evaluated against verified outages in a given time-period. Implementation of this interface should provide such a list of outages.
/// </summary>
public interface IKnownOutagesProvider
{
    /// <summary>
    /// Returns a list of outages.
    /// </summary>
    /// <returns>List of outages.</returns>
    IReadOnlyList<Outage> GetKnownOutages();

    /// <summary>
    /// Initializes provider with known outages. Returns failed <see cref="ValidationResult"/> if import failed.
    /// </summary>
    /// <returns>Result of the validation for imported data.</returns>
    Task<ValidationResult> ImportAndValidate();
}