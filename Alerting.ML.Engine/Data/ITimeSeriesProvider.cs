using System.Collections.Immutable;
using Alerting.ML.Engine.Alert;
using FluentValidation.Results;

namespace Alerting.ML.Engine.Data;

/// <summary>
///     Provides a metric data for <see cref="IAlert{T}" /> evaluation.
/// </summary>
public interface ITimeSeriesProvider
{
    /// <summary>
    ///     Returns an ordered (earliest to latest) array of metric values.
    /// </summary>
    /// <returns></returns>
    ImmutableArray<Metric> GetTimeSeries();

    /// <summary>
    ///     Represents calculated statistics from the TimeSeries Values
    /// </summary>
    TimeSeriesStatistics Statistics { get; }

    /// <summary>
    ///     Initializes provider with known outages. Returns failed <see cref="ValidationResult" /> if import failed.
    /// </summary>
    /// <returns>Result of the validation for imported data.</returns>
    Task<ValidationResult> ImportAndValidate();
}