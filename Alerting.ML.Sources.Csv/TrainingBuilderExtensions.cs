using Alerting.ML.Engine;

namespace Alerting.ML.Sources.Csv;

/// <summary>
///     Defines CSV-specific extensions for <see cref="TrainingBuilder" />
/// </summary>
public static class TrainingBuilderExtensions
{
    /// <param name="builder">Builder to modify</param>
    extension(TrainingBuilder builder)
    {
        /// <summary>
        ///     Associates builder with outages imported from CSV file at <see cref="Path" />
        /// </summary>
        /// <param name="path">Path to CSV file.</param>
        /// <returns>A new instance of <see cref="TrainingBuilder" /></returns>
        public TrainingBuilder WithCsvOutagesProvider(string path)
        {
            return builder.WithKnownOutagesProvider(new CsvOutagesProvider(path));
        }

        /// <summary>
        ///     Associates builder with time-series imported from CSV file at <see cref="Path" />
        /// </summary>
        /// <param name="path">Path to CSV file.</param>
        /// <returns>A new instance of <see cref="TrainingBuilder" /></returns>
        public TrainingBuilder WithCsvTimeSeriesProvider(string path)
        {
            return builder.WithTimeSeriesProvider(new CsvTimeSeriesProvider(path));
        }
    }
}