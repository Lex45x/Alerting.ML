namespace Alerting.ML.Engine.Alert;

[AttributeUsage(AttributeTargets.Property)]
public abstract class ConfigurationParameterAttribute : Attribute
{
    /// <summary>
    /// Allows to delay execution of value generation when parameter has dependencies. Lower order executes sooner.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Indicates that during <see cref="IConfigurationFactory{T}.Crossover"/> the value of underlying property could become invalid with respect to the whole configuration. Implementation of <see cref="CrossoverRepair"/> is mandatory.
    /// </summary>
    public virtual bool CrossoverSensitive { get; } = false;

    public virtual object CrossoverRepair(object value, IAlertConfiguration appliedTo)
    {
        return value;
    }
    public abstract object GetRandomValue(IAlertConfiguration appliedTo);
    public abstract object Nudge(object value, IAlertConfiguration appliedTo);
}