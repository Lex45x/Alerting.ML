using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine.Optimizer;
using ReactiveUI;

namespace Alerting.ML.App.Views.Training;

public class TrainingViewModel : ViewModelBase, IRoutableViewModel
{
    public TrainingViewModel(IScreen hostScreen, ITrainingSession session)
    {
        HostScreen = hostScreen;
        Session = session;
    }

    public string? UrlPathSegment => "training";
    public IScreen HostScreen { get; }
    public ITrainingSession Session { get; }
}