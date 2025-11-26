using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Csv;

public class CsvTimeSeriesProvider(string filePath) : ITimeSeriesProvider
{
    public string FileName => Path.GetFileName(FilePath);
    public string FilePath { get; } = filePath;
    public Metric[] GetTimeSeries()
    {
        throw new NotImplementedException();
    }
}