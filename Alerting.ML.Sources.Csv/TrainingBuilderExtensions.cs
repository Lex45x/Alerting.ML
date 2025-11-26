using Alerting.ML.Engine;

namespace Alerting.ML.Sources.Csv;

public static class TrainingBuilderExtensions
{
    extension(TrainingBuilder builder)
    {
        public TrainingBuilder WithCsvOutagesProvider(string path)
        {
            return builder.WithKnownOutagesProvider(new CsvOutagesProvider(path));
        }

        public TrainingBuilder WithCsvTimeSeriesProvider(string path)
        {
            return builder.WithTimeSeriesProvider(new CsvTimeSeriesProvider(path));
        }
    }
}