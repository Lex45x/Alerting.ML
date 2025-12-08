using System.Collections.Immutable;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using FluentValidation.Results;

namespace Alerting.ML.Engine.Tests.Serialization;

public class EventSerializationTest
{
    public IReadOnlyList<IEvent> Events { get; } = new List<IEvent>
    {
        new AlertScoreComputedEvent(new AlertScoreCard(precision: 0.1, TimeSpan.FromMinutes(minutes: 1),
            recall: 0.3, outagesCount: 10,
            new TestAlertConfiguration(), isNotFeasible: false), AggregateVersion: 0),
        new EvaluationCompletedEvent<TestAlertConfiguration>(new TestAlertConfiguration(),
            new List<Outage> { new(DateTime.UtcNow, DateTime.UtcNow.AddHours(value: 2)) }, AggregateVersion: 1),
        new GenerationCompletedEvent(AggregateVersion: 2),
        new OptimizerConfiguredEvent(OptimizationConfiguration.Default, AggregateVersion: 3),
        new RandomConfigurationAddedEvent<TestAlertConfiguration>(new TestAlertConfiguration(), AggregateVersion: 4),
        new SurvivorsCountedEvent<TestAlertConfiguration>(
            new List<TestAlertConfiguration> { new(), new() }, AggregateVersion: 5),
        new TournamentRoundCompletedEvent<TestAlertConfiguration>(new TestAlertConfiguration(),
            new TestAlertConfiguration(), AggregateVersion: 6),
        new StateInitializedEvent<TestAlertConfiguration>(Guid.NewGuid(), DateTime.UtcNow, "Serialization testing",
            "Testing", new TestAlert(), [new Metric(DateTime.UtcNow, value: 0)], new List<Outage>(),
            new DefaultAlertScoreCalculator(), new DefaultConfigurationFactory<TestAlertConfiguration>(),
            AggregateVersion: 7),
        new CriticalFailureEvent(AggregateVersion: 8, new ValidationResult()),
        new EvaluationCompletedEvent<TestAlertConfiguration>(new TestAlertConfiguration(), new List<Outage>(),
            AggregateVersion: 9),
        new TrainingCompletedEvent(AggregateVersion: 10)
    };

    [Test]
    public async Task WriteAndReadEvents()
    {
        KnownTypeInfoResolver.Instance.RegisterConfigurationType<TestAlertConfiguration>();
        KnownTypeInfoResolver.Instance.RegisterAlertType<TestAlert>();

        var aggregateId = Guid.NewGuid();
        var store = "./test-store";
        var jsonFileEventStore = new JsonFileEventStore(store);

        foreach (var @event in Events)
        {
            jsonFileEventStore.Write(aggregateId, @event);
        }

        jsonFileEventStore.Dispose();

        jsonFileEventStore = new JsonFileEventStore(store);

        var events = await jsonFileEventStore.GetAll(aggregateId, CancellationToken.None).ToListAsync();

        Assert.That(events.Count, Is.EqualTo(Events.Count));
    }

    [Test]
    public void EnsureAllEventsAreCovered()
    {
        foreach (var eventType in typeof(IEvent).Assembly.GetTypes()
                     .Where(type => type.IsAssignableTo(typeof(IEvent)) && !type.IsAbstract))
        {
            if (!Events.Select(@event => @event.GetType()).Any(knownType =>
                    knownType == eventType || (knownType.IsConstructedGenericType &&
                                               knownType.GetGenericTypeDefinition() == eventType)))
            {
                Assert.Fail($"Event of type {eventType} is not covered by serialization tests.");
            }
        }
    }
}

public class TestAlert : IAlert<TestAlertConfiguration>
{
    public string ProviderName => "Testing";

    public IEnumerable<Outage> Evaluate(ImmutableArray<Metric> timeSeries, TestAlertConfiguration configuration)
    {
        yield break;
    }
}

public class TestAlertConfiguration : AlertConfiguration
{
    public override bool Equals(AlertConfiguration? other)
    {
        return other?.GetType() == GetType();
    }

    public override int GetHashCode()
    {
        return 42;
    }

    public override string ToString()
    {
        return "TestAlertConfiguration";
    }

    public override double Distance(AlertConfiguration other)
    {
        return 0;
    }
}