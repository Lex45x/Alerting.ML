using Alerting.ML.Engine.Alert;

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
}