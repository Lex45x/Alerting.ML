using Alerting.ML.App.Components.Overview;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.TrainingCreation;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Alerting.ML.App.Model.Training;

namespace Alerting.ML.App.Views.Overview;

public class OverviewViewModel : ViewModelBase, IRoutableViewModel
{
    private readonly IBackgroundTrainingOrchestrator trainingOrchestrator;

    public OverviewViewModel(IScreen hostScreen, IBackgroundTrainingOrchestrator trainingOrchestrator)
    {
        this.trainingOrchestrator = trainingOrchestrator;
        HostScreen = hostScreen;
        WindowSizeChangedCommand = ReactiveCommand.Create<SizeChangedEventArgs>(WindowSizeChanged);
        NewOptimizationCommand = ReactiveCommand.Create(NewOptimization);
    }

    private void NewOptimization()
    {
        HostScreen.Router.Navigate.Execute(new TrainingCreationViewModel(HostScreen, trainingOrchestrator));
    }

    private void WindowSizeChanged(SizeChangedEventArgs e)
    {
        EffectiveWidth = e.NewSize.Width;
        EffectiveHeight = e.NewSize.Height;
    }

    public virtual ObservableCollection<TrainingCardViewModel> Cards { get; } = new();
    public ReactiveCommand<SizeChangedEventArgs, Unit> WindowSizeChangedCommand { get; }

    public ReactiveCommand<Unit, Unit> NewOptimizationCommand { get; }

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
    public int CardsRows => Math.Max((int)Math.Floor(EffectiveHeight / 200), Cards.Count / CardsColumns);

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
}

public class OverviewViewModelDesignTime : OverviewViewModel
{
    public OverviewViewModelDesignTime() : base(null, null)
    {
    }

    public override ObservableCollection<TrainingCardViewModel> Cards { get; } = new()
    {
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
        new TrainingCardViewModelDesignTime(),
    };
}