using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.Model.Training;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Scoring;
using Avalonia.Controls;
using ReactiveUI;

namespace Alerting.ML.App.Views.Training;

public class TrainingResultsViewModel : RoutableViewModelBase
{
    public TrainingResultsViewModel(IScreen hostScreen, ITrainingSession session) : base(hostScreen)
    {
        Session = session;
        GoBackCommand = ReactiveCommand.CreateFromTask(GoBack, IsOnTopOfNavigation);
        this.WhenAnyValue(model => model.Session.TopConfigurations)
            .Subscribe(cards =>
            {
                RankedScoreCards = cards.OrderByDescending(card => card.Fitness).Select((card, i) => new RankedScoreCard(i+1, card))
                    .ToList();
                BestFitness = cards.Max(card => card.Fitness);
                AveragePrecision = cards.Average(card => card.Precision);
                AverageRecall = cards.Average(card => card.Recall);
            })
            .DisposeWith(Disposables);
    }

    public ReactiveCommand<Unit, Unit> GoBackCommand { get; }

    public ITrainingSession Session
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public override string UrlPathSegment => "results";
    public double BestFitness { get; private set => this.RaiseAndSetIfChanged(ref field, value); }
    public double AveragePrecision { get; private set => this.RaiseAndSetIfChanged(ref field, value); }
    public double AverageRecall { get; private set => this.RaiseAndSetIfChanged(ref field, value); }

    private async Task GoBack()
    {
        await HostScreen.Router.NavigateBack.Execute();
    }

    public IReadOnlyList<RankedScoreCard> RankedScoreCards
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = [];

    public record RankedScoreCard(int Rank, AlertScoreCard ScoreCard);
}

public class TrainingResultsViewModelDesignTime : TrainingResultsViewModel
{
    private static readonly ITrainingSession StubSession = new DesignTimeTrainingSession();

    public TrainingResultsViewModelDesignTime() : base(null!, StubSession)
    {
    }

    
}