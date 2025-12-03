using Alerting.ML.App.Model.Training;
using Alerting.ML.App.Views.Overview;
using ReactiveUI;

namespace Alerting.ML.App.ViewModels;

public class MainWindowViewModel : ViewModelBase, IScreen
{
    private readonly IBackgroundTrainingOrchestrator trainingOrchestrator;

    public MainWindowViewModel(IBackgroundTrainingOrchestrator trainingOrchestrator)
    {
        this.trainingOrchestrator = trainingOrchestrator;
        Router.NavigateAndReset.Execute(new OverviewViewModel(this, trainingOrchestrator));
    }

    public RoutingState Router { get; } = new();
}