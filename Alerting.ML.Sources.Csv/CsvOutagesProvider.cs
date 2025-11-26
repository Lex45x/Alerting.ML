using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Csv
{
    public class CsvOutagesProvider(string filePath) : IKnownOutagesProvider
    {
        public IReadOnlyList<Outage> GetKnownOutages()
        {
            throw new NotImplementedException();
        }

        public string FileName => Path.GetFileName(FilePath);
        public string FilePath { get; } = filePath;
    }
}
