using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Csv
{
    public class CsvOutagesProvider : IKnownOutagesProvider
    {
        public IReadOnlyList<Outage> GetKnownOutages()
        {
            throw new NotImplementedException();
        }
    }
}
