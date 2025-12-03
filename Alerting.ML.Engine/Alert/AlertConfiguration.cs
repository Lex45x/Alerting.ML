namespace Alerting.ML.Engine.Alert;

/// <summary>
/// Represents a configuration for an alert rule.
/// </summary>
public abstract class AlertConfiguration : IEquatable<AlertConfiguration>
{
    /// <summary>
    /// Allows to compare contents of two different configuration instances.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract bool Equals(AlertConfiguration? other);


    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return Equals(obj as AlertConfiguration);
    }

    /// <inheritdoc />
    public abstract override int GetHashCode();

    /// <summary>
    /// Forcing all descendants to explicitly override ToString() in a debugging-friendly way.
    /// </summary>
    /// <returns>String representation of a given configuration</returns>
    public abstract override string ToString();

    /// <summary>
    /// Measures Euclidean distance between two configurations. Can be used to calculate population diversity. 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public abstract double Distance(AlertConfiguration other);
}