using Alerting.ML.App.Model.Training;
using Alerting.ML.App.Views.Overview;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace Alerting.ML.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly IBackgroundTrainingOrchestrator trainingOrchestrator;
    public RoutingState Router { get; } = new();

    public MainWindowViewModel(IBackgroundTrainingOrchestrator trainingOrchestrator)
    {
        this.trainingOrchestrator = trainingOrchestrator;
        Router.NavigateAndReset.Execute(new OverviewViewModel(this, trainingOrchestrator));
    }
}