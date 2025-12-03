using Alerting.ML.Engine.Data;
using FluentValidation.Results;

namespace Alerting.ML.Sources.Csv;

/// <summary>
///     Reads known outages from CSV file <paramref name="filePath" />
/// </summary>
/// <param name="filePath">Path to CSV file.</param>
public class CsvOutagesProvider(string filePath) : IKnownOutagesProvider
{
    private List<Outage>? outages;

    /// <summary>
    ///     <see cref="Path.GetFileName(string?)" /> applied to <see cref="FilePath" />
    /// </summary>
    public string? FileName => Path.GetFileName(FilePath);

    /// <summary>
    ///     Full path to selected CSV file.
    /// </summary>
    public string? FilePath { get; } = filePath;

    /// <inheritdoc />
    public IReadOnlyList<Outage> GetKnownOutages()
    {
        return outages ?? throw new InvalidOperationException(
            $"{nameof(CsvOutagesProvider)} is not initialized or invalid! Make sure to call {nameof(ImportAndValidate)} before attempting to retrieve outages.");
    }

    /// <inheritdoc />
    public async Task<ValidationResult> ImportAndValidate()
    {
        if (string.IsNullOrWhiteSpace(FilePath))
        {
            return new ValidationResult([new ValidationFailure(nameof(FilePath), "CSV File path is null or empty!")]);
        }

        await using var fileStream = File.OpenRead(FilePath);
        using var reader = new StreamReader(fileStream);
        var result = new List<Outage>();
        var lineIndex = 0;
        var headerHasData = false;
        var outageStartIndex = -1;
        var outageEndIndex = -1;

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
                    "Unable to identify CSV file separator. Make sure to use one of the supported values: ';' ',''\\t'")
            ]);
        }

        // check if header has column names or date-time data.
        var firstLineParts = currentLine.Split(csvSeparator);

        for (var index = 0; index < firstLineParts.Length; index++)
        {
            var part = firstLineParts[index];
            if (!DateTime.TryParse(part, out var date))
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
            if (!DateTime.TryParse(part, out _))
            {
                continue;
            }

            if (outageStartIndex < 0)
            {
                outageStartIndex = index;
            }
            else if (outageEndIndex < 0)
            {
                outageEndIndex = index;
            }
            else
            {
                return new ValidationResult([
                    new ValidationFailure(nameof(FilePath),
                        $"Line #{lineIndex + 1} contains more then 2 date-time columns. CSV with outages must contain exactly 2 date-time values on each row.")
                ]);
            }
        }

        //ensure both date columns are identified.
        if (outageStartIndex < 0 || outageEndIndex < 0)
        {
            return new ValidationResult([
                new ValidationFailure(nameof(FilePath),
                    $"Line #{lineIndex + 1} contains less than 2 date-time columns. CSV with outages must contain exactly 2 date-time values on each row.")
            ]);
        }

        // identify which index corresponds to the start of the outage
        (outageStartIndex, outageEndIndex) =
            DateTime.Parse(firstDataLineParts[outageStartIndex]) <
            DateTime.Parse(firstDataLineParts[outageEndIndex])
                ? (outageStartIndex, outageEndIndex)
                : (outageEndIndex, outageStartIndex);


        // proceed to CSV parsing
        var errorList = new List<ValidationFailure>();
        while (!string.IsNullOrWhiteSpace(currentLine))
        {
            var rowParts = currentLine.Split(csvSeparator);

            if (rowParts.Length > outageStartIndex && rowParts.Length > outageEndIndex)
            {
                if (!DateTime.TryParse(rowParts[outageStartIndex], out var outageStart))
                {
                    errorList.Add(new ValidationFailure(nameof(FilePath),
                        $"Line #{lineIndex + 1} contains invalid date time at position {outageStartIndex}."));
                }

                if (!DateTime.TryParse(rowParts[outageEndIndex], out var outageEnd))
                {
                    errorList.Add(new ValidationFailure(nameof(FilePath),
                        $"Line #{lineIndex + 1} contains invalid date time at position {outageEndIndex}."));
                }

                if (outageEnd < outageStart)
                {
                    errorList.Add(new ValidationFailure(nameof(FilePath),
                        $"Line #{lineIndex + 1} has outage start and outage end in invalid order."));
                }

                result.Add(new Outage(outageStart, outageEnd));
            }
            else
            {
                errorList.Add(new ValidationFailure(nameof(FilePath),
                    $"Line #{lineIndex + 1} contains invalid amount of elements. Expected to find a date time at indexes {outageStartIndex} and {outageEndIndex}"));
            }

            currentLine = await reader.ReadLineAsync();
            lineIndex += 1;
        }

        if (errorList.Count > 0)
        {
            return new ValidationResult(errorList);
        }

        outages = result;
        return new ValidationResult();
    }
}