namespace Alerting.ML.Engine.Alert;

/// <summary>
/// Base attribute for all <see cref="AlertConfiguration"/> properties. Consumed by <see cref="DefaultConfigurationFactory{T}"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public abstract class ConfigurationParameterAttribute : Attribute
{
    /// <summary>
    /// Allows to delay execution of value generation when parameter has dependencies. Lower order executes sooner.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Defines an action needed to be taken to repair the value of underlying property if it can be damaged by <see cref="IConfigurationFactory{T}.Crossover"/>.
    /// By default, no changes is taken. Override is necessary if current property value depends on other properties.
    /// </summary>
    /// <param name="value">Value of the property.</param>
    /// <param name="appliedTo">An instance of configuration this property belongs to.</param>
    /// <returns></returns>
    public virtual object CrossoverRepair(object value, AlertConfiguration appliedTo)
    {
        return value;
    }

    /// <summary>
    /// Generates a random value for a given property.
    /// </summary>
    /// <param name="appliedTo">An instance of configuration this property belongs to.</param>
    /// <returns></returns>
    public abstract object GetRandomValue(AlertConfiguration appliedTo);

    /// <summary>
    /// Applies a small change to the value of property. Used in <see cref="DefaultConfigurationFactory{T}.Mutate"/>.
    /// </summary>
    /// <param name="value">Value of the property.</param>
    /// <param name="appliedTo">An instance of configuration this property belongs to.</param>
    /// <returns></returns>
    public abstract object Nudge(object value, AlertConfiguration appliedTo);
}