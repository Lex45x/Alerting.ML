using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Tests.Serialization;

public class EventSerializationTest
{
    public IReadOnlyList<IEvent> Events { get; } = new List<IEvent>
    {
        new AlertScoreComputedEvent(new AlertScoreCard(precision: 0.1, TimeSpan.FromMinutes(minutes: 1),
            falseNegativeRate: 0.3, outagesCount: 10,
            new TestAlertConfiguration(), isNotFeasible: false), AggregateVersion: 0),
        new EvaluationCompletedEvent<TestAlertConfiguration>(new TestAlertConfiguration(),
            new List<Outage> { new(DateTime.UtcNow, DateTime.UtcNow.AddHours(value: 2)) }, AggregateVersion: 1),
        new GenerationCompletedEvent(AggregateVersion: 2),
        new OptimizerConfiguredEvent(OptimizationConfiguration.Default, AggregateVersion: 3),
        new RandomConfigurationAddedEvent<TestAlertConfiguration>(new TestAlertConfiguration(), AggregateVersion: 4),
        new SurvivorsCountedEvent<TestAlertConfiguration>(
            new List<TestAlertConfiguration> { new(), new() }, AggregateVersion: 5),
        new TournamentRoundCompletedEvent<TestAlertConfiguration>(new TestAlertConfiguration(),
            new TestAlertConfiguration(), AggregateVersion: 6)
    };

    [Test]
    public async Task WriteAndReadEvents()
    {
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

        Assert.That(events, Is.EquivalentTo(Events));
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