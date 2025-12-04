using ReactiveUI;

namespace Alerting.ML.App.DesignTimeExtensions;

public static class DesignTime
{
    public static IScreen MockScreen { get; } = new MockScreen();


}

internal class MockScreen : IScreen
{
    public RoutingState Router { get; } = new();
}