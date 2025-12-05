using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using ReactiveUI;

namespace Alerting.ML.App.Views.Training;

public class TrainingViewModel : RoutableViewModelBase
{
    public TrainingViewModel(IScreen hostScreen, ITrainingSession session) : base(hostScreen)
    {
        Session = session;
        GoBackCommand = ReactiveCommand.CreateFromTask(GoBack, IsOnTopOfNavigation);

        session.WhenAnyValue(trainingSession => trainingSession.CurrentConfiguration)
            .Subscribe(configuration =>
            {
                if (configuration != null)
                {
                    ConfigurationBuilder = TrainingConfigurationBuilder.FromExisting(configuration);
                }
            })
            .DisposeWith(Disposables);

        ResumeCommand =
            ReactiveCommand.Create(() => session.Start(ConfigurationBuilder.Apply(session.CurrentConfiguration!)),
                this.WhenAnyValue(model => model.Session.State, state => state == TrainingState.Paused));
        PauseCommand = ReactiveCommand.Create(session.Stop,
            this.WhenAnyValue(model => model.Session.State, state => state == TrainingState.Training));
    }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }
    public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
    public ReactiveCommand<Unit, Unit> PauseCommand { get; }

    public ITrainingSession Session
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TrainingConfigurationBuilder ConfigurationBuilder
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = new();


    public override string UrlPathSegment => "training";

    private async Task GoBack()
    {
        await HostScreen.Router.NavigateBack.Execute();
    }
}

public class TrainingViewModelDesignTime : TrainingViewModel
{
    private static readonly ITrainingSession StubSession = new DesignTimeTrainingSession();

    public TrainingViewModelDesignTime() : base(null!, StubSession)
    {
    }
}

internal class DesignTimeTrainingSession : ITrainingSession
{
    public Guid Id => Guid.NewGuid();
    public string Name => "Design-time Training";
    public ObservableCollection<double> PopulationDiversity { get; } = [12, 3, 5, 99, 3.5, 6, 1];
    public ObservableCollection<double> AverageGenerationFitness { get; } = [0.3, 0.1, 0.5, 0.6, 0.66, 0.7];

    public ObservableCollection<double> BestGenerationFitness { get; } = [0.4, 0.4, 0.55, 0.65, 0.8, 0.91];

    // todo: will be initialized later for other views
    public ObservableCollection<AlertScoreCard> TopConfigurations { get; } = [];
    public int CurrentGeneration => 25;
    public double BestFitness => 0.7;
    public double FitnessDiff => 0.03;
    public int TotalEvaluations => 2500;
    public double ProgressPercentage => 25.0 / CurrentConfiguration.TotalGenerations;
    public CloudProvider AlertProvider => CloudProvider.Azure;
    public DateTime CreatedAt => DateTime.UtcNow;
    public TimeSpan Elapsed => TimeSpan.FromSeconds(seconds: 124);
    public double RemainingMinutes => 12.5;
    public OptimizationConfiguration CurrentConfiguration => OptimizationConfiguration.Default;

    public void Start(OptimizationConfiguration configuration)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }


    public Task Hydrate(Guid aggregateId)
    {
        throw new NotImplementedException();
    }

    public TrainingState State => TrainingState.Paused;
}