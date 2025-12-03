using Alerting.ML.Sources.Csv;

namespace Alerting.ML.Source.Csv.Tests;

public class OutagesCsvImportTest
{
    public static IEnumerable<object[]> SampleOutagesCsv()
    {
        foreach (var csvFile in Directory.EnumerateFiles("SampleOutageCsv"))
        {
            var fileName = Path.GetFileName(csvFile);
            if (fileName.StartsWith("valid"))
            {
                yield return [csvFile, true];
            }
            else
            {
                yield return [csvFile, false];
            }
        }
    }

    [Test]
    [TestCaseSource(nameof(SampleOutagesCsv))]
    public async Task ImportFromCsv(string path, bool canImport)
    {
        var csvOutagesProvider = new CsvOutagesProvider(path);

        var validationResult = await csvOutagesProvider.ImportAndValidate();

        Assert.That(validationResult.IsValid, Is.EqualTo(canImport));
    }
}