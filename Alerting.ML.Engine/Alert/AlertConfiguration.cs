namespace Alerting.ML.Engine.Alert;

public abstract class AlertConfiguration<T> : IAlertConfiguration where T : AlertConfiguration<T>
{
    public abstract override string ToString();
}