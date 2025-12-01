using Alerting.ML.App.Components.Overview;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.Training;
using Alerting.ML.App.Views.TrainingCreation;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;

namespace Alerting.ML.App.Views.Overview;

public class OverviewViewModel : ViewModelBase, IRoutableViewModel
{
    public OverviewViewModel(IScreen hostScreen, IBackgroundTrainingOrchestrator trainingOrchestrator)
    {
        this.TrainingOrchestrator = trainingOrchestrator;
        HostScreen = hostScreen;
        WindowSizeChangedCommand = ReactiveCommand.Create<SizeChangedEventArgs>(WindowSizeChanged);
        NewOptimizationCommand = ReactiveCommand.Create(NewOptimization);
        OpenSessionCommand = ReactiveCommand.Create<ITrainingSession>(OpenSession);
    }

    private void OpenSession(ITrainingSession session)
    {
        HostScreen.Router.Navigate.Execute(new TrainingViewModel(HostScreen, session));
    }

    private void NewOptimization()
    {
        HostScreen.Router.Navigate.Execute(new TrainingCreationViewModel(HostScreen, TrainingOrchestrator));
    }

    private void WindowSizeChanged(SizeChangedEventArgs e)
    {
        EffectiveWidth = e.NewSize.Width;
        EffectiveHeight = e.NewSize.Height;
    }

    public ReactiveCommand<SizeChangedEventArgs, Unit> WindowSizeChangedCommand { get; }

    public ReactiveCommand<Unit, Unit> NewOptimizationCommand { get; }
    public ReactiveCommand<ITrainingSession, Unit> OpenSessionCommand { get; }

    public double EffectiveWidth
    {
        get;
        set
        {
            field = value;
            this.RaisePropertyChanged(nameof(CardsColumns));
        }
    }

    public int CardsColumns => Math.Max((int)Math.Floor(EffectiveWidth / 450), 1);
    public int CardsRows => Math.Max((int)Math.Floor(EffectiveHeight / 200), TrainingOrchestrator.AllSessions.Count / CardsColumns);

    public double EffectiveHeight
    {
        get;
        set
        {
            field = value;
            this.RaisePropertyChanged(nameof(CardsRows));
        }
    }

    public string? UrlPathSegment => "overview";
    public IScreen HostScreen { get; }

    public IBackgroundTrainingOrchestrator TrainingOrchestrator { get; }
}

public class OverviewViewModelDesignTime : OverviewViewModel
{
    public OverviewViewModelDesignTime() : base(null, null)
    {
    }

}