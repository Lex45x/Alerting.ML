using System.Collections.Immutable;
using System.Text.Json;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using Alerting.ML.Sources.Csv;
using BenchmarkDotNet.Attributes;

namespace Alerting.ML.Benchmarking.Serialization;

[MemoryDiagnoser]
public class StateInitializedEventSerialization
{
    public StateInitializedEvent<SampleConfiguration> Event { get; set; }

    [GlobalSetup]
    public async Task Setup()
    {
        var csvTimeSeriesProvider = new CsvTimeSeriesProvider("SampleInputs/timeseries_1.csv");
        await csvTimeSeriesProvider.ImportAndValidate();

        var csvOutagesProvider = new CsvOutagesProvider("SampleInputs/outages_1.csv");
        await csvOutagesProvider.ImportAndValidate();

        KnownTypeInfoResolver.Instance.RegisterConfigurationType<SampleConfiguration>();
        KnownTypeInfoResolver.Instance.RegisterAlertType<SampleAlert>();

        Event = new StateInitializedEvent<SampleConfiguration>(Guid.NewGuid(), DateTime.UtcNow, "Benchmarking", "None",
            new SampleAlert(), csvTimeSeriesProvider.GetTimeSeries(), csvOutagesProvider.GetKnownOutages(),
            new DefaultAlertScoreCalculator(), new DefaultConfigurationFactory<SampleConfiguration>(csvTimeSeriesProvider.Statistics),
            AggregateVersion: 0);
    }

    [Benchmark]
    public async Task<ImmutableArray<Metric>> ReadFromCsv()
    {
        var csvTimeSeriesProvider = new CsvTimeSeriesProvider("SampleInputs/timeseries_1.csv");
        await csvTimeSeriesProvider.ImportAndValidate();

        return csvTimeSeriesProvider.GetTimeSeries();
    }

    [Benchmark(Baseline = true)]
    public IEvent CurrentSerialization()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            TypeInfoResolver = KnownTypeInfoResolver.Instance,
            Converters = { new MetricsListConverter() }
        };

        var eventString = JsonSerializer.Serialize(Event, jsonSerializerOptions);

        var @event =
            JsonSerializer.Deserialize<StateInitializedEvent<SampleConfiguration>>(eventString, jsonSerializerOptions);

        return @event!;
    }
}

public class SampleAlert : IAlert<SampleConfiguration>
{
    public string ProviderName => "None";

    public IEnumerable<Outage> Evaluate(ImmutableArray<Metric> timeSeries, SampleConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}

public class SampleConfiguration : AlertConfiguration
{
    public override bool Equals(AlertConfiguration? other)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        throw new NotImplementedException();
    }

    public override double Distance(AlertConfiguration other)
    {
        throw new NotImplementedException();
    }
}