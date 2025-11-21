using System.Reactive;
using Alerting.ML.App.Components.Overview;
using Alerting.ML.App.Views.Overview;
using ReactiveUI;

namespace Alerting.ML.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    public RoutingState Router { get; } = new();

    public MainWindowViewModel()
    {
        Router.NavigateAndReset.Execute(new OverviewViewModel(this));
    }
}