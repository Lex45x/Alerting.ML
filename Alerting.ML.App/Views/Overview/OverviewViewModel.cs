using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.DesignTimeExtensions;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.App.Views.Training;
using Alerting.ML.App.Views.TrainingCreation;
using Avalonia.Controls;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace Alerting.ML.App.Views.Overview;

public class OverviewViewModel : RoutableViewModelBase
{
    private readonly ReadOnlyObservableCollection<ITrainingSession> cards;

    public OverviewViewModel(IScreen hostScreen, IBackgroundTrainingOrchestrator trainingOrchestrator) :
        base(hostScreen)
    {
        TrainingOrchestrator = trainingOrchestrator;

        WindowSizeChangedCommand = ReactiveCommand.Create<SizeChangedEventArgs>(WindowSizeChanged);
        NewOptimizationCommand = ReactiveCommand.CreateFromObservable(() =>
                HostScreen.Router.Navigate.Execute(new TrainingCreationViewModel(HostScreen, TrainingOrchestrator)),
            IsOnTopOfNavigation);

        OpenSessionCommand = ReactiveCommand.CreateFromTask<ITrainingSession>(OpenSession, IsOnTopOfNavigation);

        var searchPhraseFilter = this.WhenAnyValue(model => model.SearchPhrase)
            .Select<string?, Func<ITrainingSession, bool>>(s => session =>
                string.IsNullOrWhiteSpace(s) ||
                (session.Name?.Contains(s, StringComparison.OrdinalIgnoreCase) ?? true));

        var providerFilter = this.WhenAnyValue(model => model.IsAllProvidersSelected,
                model => model.IsAzureProviderSelected, model => model.IsAwsProviderSelected,
                model => model.IsGcpProviderSelected)
            .Select<(bool IsAllProvidersSelected, bool IsAzureProviderSelected, bool IsAwsProviderSelected, bool IsGcpProviderSelected), Func<ITrainingSession, bool>>(tuple => session =>
                session.AlertProvider switch
                {
                    CloudProvider.Azure => tuple.IsAllProvidersSelected || tuple.IsAzureProviderSelected,
                    CloudProvider.Amazon => tuple.IsAllProvidersSelected || tuple.IsAwsProviderSelected,
                    CloudProvider.Google => tuple.IsAllProvidersSelected || tuple.IsGcpProviderSelected,
                    _ => tuple.IsAllProvidersSelected
                });

        TrainingOrchestrator.AllSessions
            .ToObservableChangeSet()
            .Filter(providerFilter)
            .Filter(searchPhraseFilter)
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out cards)
            .Subscribe()
            .DisposeWith(Disposables);
    }

    public virtual ReadOnlyObservableCollection<ITrainingSession> Cards => cards;

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
        switch (session.State)
        {
            case TrainingState.Loading:
            case TrainingState.Training:
            case TrainingState.Paused:
                await HostScreen.Router.Navigate.Execute(new TrainingViewModel(HostScreen, session));
                break;
            case TrainingState.Completed:
            case TrainingState.Failed:
                await HostScreen.Router.Navigate.Execute(new TrainingResultsViewModel(HostScreen, session));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void WindowSizeChanged(SizeChangedEventArgs e)
    {
        EffectiveWidth = e.NewSize.Width;
        EffectiveHeight = e.NewSize.Height;
    }
}

public class OverviewViewModelDesignTime : OverviewViewModel
{
    public OverviewViewModelDesignTime() : base(DesignTime.MockScreen, DesignTime.MockOrchestrator)
    {
    }

    public override ReadOnlyObservableCollection<ITrainingSession> Cards { get; } =
        new([
            new DesignTimeTrainingSession(),
            new DesignTimeTrainingSession(),
            new DesignTimeTrainingSession(),
            new DesignTimeTrainingSession(),
            new DesignTimeTrainingSession(),
            new DesignTimeTrainingSession(),
            new DesignTimeTrainingSession()
        ]);
}