namespace Alerting.ML.Engine.Data;

public interface IKnownOutagesProvider
{
    IReadOnlyList<Outage> GetKnownOutages();
}