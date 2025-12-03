using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using NUnit.Framework.Constraints;
using NUnit.Framework.Legacy;

namespace Alerting.ML.Engine.Tests.Serialization;

public class EventSerializationTest
{
    public IReadOnlyList<IEvent> Events { get; } = new List<IEvent>
    {
        new AlertScoreComputedEvent(new AlertScoreCard(0.1, TimeSpan.FromMinutes(1), 0.3, 10,
            new TestAlertConfiguration(), false), 0),
        new EvaluationCompletedEvent<TestAlertConfiguration>(new TestAlertConfiguration(),
            new List<Outage> { new Outage(DateTime.UtcNow, DateTime.UtcNow.AddHours(2)) }, 1),
        new GenerationCompletedEvent(2),
        new OptimizerConfiguredEvent(OptimizationConfiguration.Default, 3),
        new RandomConfigurationAddedEvent<TestAlertConfiguration>(new TestAlertConfiguration(), 4),
        new SurvivorsCountedEvent<TestAlertConfiguration>(
            new List<TestAlertConfiguration> { new TestAlertConfiguration(), new TestAlertConfiguration() }, 5),
        new TournamentRoundCompletedEvent<TestAlertConfiguration>(new TestAlertConfiguration(),
            new TestAlertConfiguration(), 6)
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

        CollectionAssert.AreEqual(Events, @events);
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