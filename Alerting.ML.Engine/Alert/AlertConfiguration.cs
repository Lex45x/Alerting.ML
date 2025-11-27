namespace Alerting.ML.Engine.Alert;

/// <summary>
/// Represents a configuration for an alert rule.
/// </summary>
public abstract class AlertConfiguration
{
    /// <summary>
    /// Forcing all descendants to explicitly override ToString() in a debugging-friendly way.
    /// </summary>
    /// <returns>String representation of a given configuration</returns>
    public abstract override string ToString();
}