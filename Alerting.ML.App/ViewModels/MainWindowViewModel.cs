using System.Reactive;
using Alerting.ML.App.Components.Overview;
using Alerting.ML.App.Views.Overview;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Alerting.ML.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly ILoggerFactory loggerFactory;
    public RoutingState Router { get; } = new();

    public MainWindowViewModel(ILoggerFactory loggerFactory)
    {
        this.loggerFactory = loggerFactory;
        Router.NavigateAndReset.Execute(new OverviewViewModel(this, loggerFactory));
    }
}