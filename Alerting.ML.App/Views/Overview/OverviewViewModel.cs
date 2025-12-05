using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.Training;
using Alerting.ML.App.Views.TrainingCreation;
using Avalonia.Controls;
using ReactiveUI;

namespace Alerting.ML.App.Views.Overview;

public class OverviewViewModel : RoutableViewModelBase
{
    public OverviewViewModel(IScreen hostScreen, IBackgroundTrainingOrchestrator trainingOrchestrator) :
        base(hostScreen)
    {
        TrainingOrchestrator = trainingOrchestrator;

        WindowSizeChangedCommand = ReactiveCommand.Create<SizeChangedEventArgs>(WindowSizeChanged);
        NewOptimizationCommand = ReactiveCommand.CreateFromObservable(() =>
                HostScreen.Router.Navigate.Execute(new TrainingCreationViewModel(HostScreen, TrainingOrchestrator)),
            IsOnTopOfNavigation);

        OpenSessionCommand = ReactiveCommand.CreateFromTask<ITrainingSession>(OpenSession, IsOnTopOfNavigation);

        this.WhenAnyValue(model => model.TrainingOrchestrator.AllSessions,
            model => model.IsAllProvidersSelected,
            model => model.IsAzureProviderSelected,
            model => model.IsAwsProviderSelected,
            model => model.IsGcpProviderSelected,
            model => model.SearchPhrase)
            .Subscribe(tuple => this.RaisePropertyChanged(nameof(Cards)))
            .DisposeWith(Disposables);
    }

    public IReadOnlyList<ITrainingSession> Cards =>
        TrainingOrchestrator.AllSessions
            .Where(session => string.IsNullOrWhiteSpace(SearchPhrase) || session.Name.Contains(SearchPhrase, StringComparison.OrdinalIgnoreCase)).Where(session =>
                session.AlertProvider switch
                {
                    CloudProvider.Azure => IsAllProvidersSelected || IsAzureProviderSelected,
                    CloudProvider.Amazon => IsAllProvidersSelected || IsAwsProviderSelected,
                    CloudProvider.Google => IsAllProvidersSelected || IsGcpProviderSelected,
                    _ => IsAllProvidersSelected
                }).ToList();

    public string? SearchPhrase
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsAllProvidersSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;

    public bool IsAzureProviderSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsAwsProviderSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsGcpProviderSelected
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ReactiveCommand<SizeChangedEventArgs, Unit> WindowSizeChangedCommand { get; }

    public ReactiveCommand<Unit, IRoutableViewModel> NewOptimizationCommand { get; }
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

    public int CardsColumns => Math.Max((int)Math.Floor(EffectiveWidth / 450), val2: 1);

    public int CardsRows => Math.Max((int)Math.Floor(EffectiveHeight / 200),
        TrainingOrchestrator.AllSessions.Count / CardsColumns);

    public double EffectiveHeight
    {
        get;
        set
        {
            field = value;
            this.RaisePropertyChanged(nameof(CardsRows));
        }
    }

    public IBackgroundTrainingOrchestrator TrainingOrchestrator { get; }

    public override string UrlPathSegment => "overview";

    private async Task OpenSession(ITrainingSession session)
    {
        await HostScreen.Router.Navigate.Execute(new TrainingViewModel(HostScreen, session));
    }

    private void WindowSizeChanged(SizeChangedEventArgs e)
    {
        EffectiveWidth = e.NewSize.Width;
        EffectiveHeight = e.NewSize.Height;
    }
}

public class OverviewViewModelDesignTime : OverviewViewModel
{
    public OverviewViewModelDesignTime() : base(null!, null!)
    {
    }
}