using Alerting.ML.Engine.Data;

namespace Alerting.ML.Engine.Alert;

/// <summary>
///     Base attribute for all <see cref="AlertConfiguration" /> properties. Consumed by
///     <see cref="DefaultConfigurationFactory{T}" />.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class ConfigurationParameterAttribute : Attribute
{
    /// <summary>
    ///     Allows to delay execution of value generation when parameter has dependencies. Lower order executes sooner.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     Defines an action needed to be taken to repair the value of underlying property if it can be damaged by
    ///     <see cref="IConfigurationFactory{T}.Crossover" />.
    ///     By default, no changes is taken. Override is necessary if current property value depends on other properties.
    /// </summary>
    /// <param name="value">Value of the property.</param>
    /// <param name="appliedTo">An instance of configuration this property belongs to.</param>
    /// <returns></returns>
    public virtual object CrossoverRepair(object value, AlertConfiguration appliedTo)
    {
        return value;
    }

    /// <summary>
    ///     Generates a random value for a given property.
    /// </summary>
    /// <param name="appliedTo">An instance of configuration this property belongs to.</param>
    /// <param name="statistics">Statistics of the current time-series.</param>
    /// <returns></returns>
    public abstract object GetRandomValue(AlertConfiguration appliedTo, TimeSeriesStatistics statistics);

    /// <summary>
    ///     Applies a small change to the value of property. Used in <see cref="DefaultConfigurationFactory{T}.Mutate" />.
    /// </summary>
    /// <param name="value">Value of the property.</param>
    /// <param name="appliedTo">An instance of configuration this property belongs to.</param>
    /// <param name="statistics"></param>
    /// <returns></returns>
    public abstract object Nudge(object value, AlertConfiguration appliedTo, TimeSeriesStatistics statistics);
}

/// <summary>
/// Represents statistics retrieved from <see cref="ITimeSeriesProvider.GetTimeSeries"/> and can be used to guide random value generation.
/// </summary>
/// <param name="Minimum">Minimum value of the metric.</param>
/// <param name="Maximum">Maximum value of the metric.</param>
public record TimeSeriesStatistics(double Minimum, double Maximum)
{
}