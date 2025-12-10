using Alerting.ML.App.Model.Training;
using Alerting.ML.Engine.Storage;
using ReactiveUI;

namespace Alerting.ML.App.DesignTimeExtensions;

public static class DesignTime
{
    public static IScreen MockScreen { get; } = new MockScreen();

    public static IBackgroundTrainingOrchestrator MockOrchestrator { get; } =
        new BackgroundTrainingOrchestrator(new InMemoryEventStore());
}

internal class MockScreen : IScreen
{
    public RoutingState Router { get; } = new();
}