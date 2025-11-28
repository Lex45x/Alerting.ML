using Alerting.ML.Engine.Data;

namespace Alerting.ML.Sources.Csv
{
    /// <summary>
    /// Reads known outages from CSV file <paramref name="filePath"/>
    /// </summary>
    /// <param name="filePath">Path to CSV file.</param>
    public class CsvOutagesProvider(string filePath) : IKnownOutagesProvider
    {
        /// <inheritdoc />
        public IReadOnlyList<Outage> GetKnownOutages()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="Path.GetFileName(string?)"/> applied to <see cref="FilePath"/> 
        /// </summary>
        public string FileName => Path.GetFileName(FilePath);
        /// <summary>
        /// Full path to selected CSV file.
        /// </summary>
        public string FilePath { get; } = filePath;
    }
}
