using System.Collections.Immutable;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using FluentValidation.Results;

namespace Alerting.ML.Sources.Csv;

/// <summary>
///     Reads known outages from CSV file <paramref name="filePath" />
/// </summary>
/// <param name="filePath">Path to CSV file.</param>
public class CsvTimeSeriesProvider(string filePath) : ITimeSeriesProvider
{
    private ImmutableArray<Metric>? metrics;
    private TimeSeriesStatistics? statistics;

    /// <summary>
    ///     <see cref="Path.GetFileName(string?)" /> applied to <see cref="FilePath" />
    /// </summary>
    public string FileName => Path.GetFileName(FilePath);

    /// <summary>
    ///     Full path to selected CSV file.
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <inheritdoc />
    public ImmutableArray<Metric> GetTimeSeries()
    {
        return metrics ?? throw new InvalidOperationException(
            $"{nameof(CsvTimeSeriesProvider)} is not initialized or invalid! Make sure to call {nameof(ImportAndValidate)} before attempting to retrieve time-series.");
    }

    /// <inheritdoc />
    public TimeSeriesStatistics Statistics => statistics ?? throw new InvalidOperationException(
        $"{nameof(CsvTimeSeriesProvider)} is not initialized or invalid! Make sure to call {nameof(ImportAndValidate)} before attempting to retrieve time-series.");

    /// <inheritdoc />
    public async Task<ValidationResult> ImportAndValidate()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            return new ValidationResult([new ValidationFailure(nameof(FilePath), "CSV File path is null or empty!")]);
        }

        await using var fileStream = File.OpenRead(FilePath);
        using var reader = new StreamReader(fileStream);
        var result = new List<Metric>();
        var lineIndex = 0;
        var headerHasData = false;
        var timestampIndex = -1;
        var valueIndex = -1;

        var currentLine = await reader.ReadLineAsync();

        if (currentLine == null)
        {
            return new ValidationResult([new ValidationFailure(nameof(FilePath), "CSV File is empty!")]);
        }

        //identify separator in the file

        var csvSeparator = CsvConstants.SupportedSeparators
            .Where(separator => currentLine.Split(separator).Length > 1)
            .Select(separator => separator.ToString())
            .FirstOrDefault();

        if (csvSeparator == null)
        {
            return new ValidationResult([
                new ValidationFailure(nameof(FilePath),
                    "Unable to identify CSV file separator. Make sure to use one of the supported values: ';' ',' '\\t'")
            ]);
        }

        // check if header has column names or date-time data.
        var firstLineParts = currentLine.Split(csvSeparator);

        for (var index = 0; index < firstLineParts.Length; index++)
        {
            var part = firstLineParts[index];
            if (!DateTime.TryParse(part, out _) && !double.TryParse(part, out _))
            {
                continue;
            }

            headerHasData = true;
        }

        if (!headerHasData)
        {
            // advance to the next CSV line.
            lineIndex += 1;

            currentLine = await reader.ReadLineAsync();

            if (currentLine == null)
            {
                return new ValidationResult([
                    new ValidationFailure(nameof(FilePath),
                        "CSV File contains only header row and no data!")
                ]);
            }
        }

        // identify indices of start and end dates
        var firstDataLineParts = currentLine.Split(csvSeparator);

        for (var index = 0; index < firstDataLineParts.Length; index++)
        {
            var part = firstDataLineParts[index];
            if (DateTime.TryParse(part, out _))
            {
                if (timestampIndex < 0)
                {
                    timestampIndex = index;
                }
                else
                {
                    return new ValidationResult([
                        new ValidationFailure(nameof(FilePath),
                            $"Line #{lineIndex + 1} contains more than 1 date-time column. CSV with time-series must contain exactly 1 date-time value on each row.")
                    ]);
                }
            }

            if (double.TryParse(part, out _))
            {
                if (valueIndex < 0)
                {
                    valueIndex = index;
                }
                else
                {
                    return new ValidationResult([
                        new ValidationFailure(nameof(FilePath),
                            $"Line #{lineIndex + 1} contains more than 1 double column. CSV with time-series must contain exactly 1 double value on each row.")
                    ]);
                }
            }
        }

        //ensure both or none date columns are identified.
        if (timestampIndex < 0 || valueIndex < 0)
        {
            return new ValidationResult([
                new ValidationFailure(nameof(FilePath),
                    $"Line #{lineIndex + 1} does not contain both necessary columns. CSV with time-series must contain exactly 1 date-time value and 1 double value on each row.")
            ]);
        }

        // proceed to CSV parsing
        var errorList = new List<ValidationFailure>();

        double? minMetricValue = null;
        double? maxMetricValue = null;

        while (!string.IsNullOrWhiteSpace(currentLine))
        {
            var rowParts = currentLine.Split(csvSeparator);

            if (rowParts.Length > timestampIndex && rowParts.Length > valueIndex)
            {
                if (!DateTime.TryParse(rowParts[timestampIndex], out var timestamp))
                {
                    errorList.Add(new ValidationFailure(nameof(FilePath),
                        $"Line #{lineIndex + 1} contains invalid date time at position {timestampIndex}."));
                }

                if (!double.TryParse(rowParts[valueIndex], out var value))
                {
                    errorList.Add(new ValidationFailure(nameof(FilePath),
                        $"Line #{lineIndex + 1} contains invalid double at position {valueIndex}."));
                }

                minMetricValue = minMetricValue.HasValue ? Math.Min(minMetricValue.Value, value) : value;
                maxMetricValue = maxMetricValue.HasValue ? Math.Max(maxMetricValue.Value, value) : value;

                result.Add(new Metric(timestamp, value));
            }
            else
            {
                errorList.Add(new ValidationFailure(nameof(FilePath),
                    $"Line #{lineIndex + 1} contains invalid amount of elements. Expected to find a date time and double at indexes {timestampIndex} and {valueIndex}"));
            }

            currentLine = await reader.ReadLineAsync();
            lineIndex += 1;
        }

        if (errorList.Count > 0)
        {
            return new ValidationResult(errorList);
        }

        metrics = [..result.OrderBy(metric => metric.Timestamp)];
        statistics = new TimeSeriesStatistics(minMetricValue!.Value, maxMetricValue!.Value);

        return new ValidationResult();
    }
}