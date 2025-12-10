using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using Avalonia.Threading;
using ReactiveUI;

namespace Alerting.ML.App.Model.Training;

public class TrainingSession : ViewModelBase, ITrainingSession
{
    private readonly List<AlertScoreCard> currentGenerationScores = [];
    private readonly DispatcherTimer dispatcherTimer;
    private readonly IGeneticOptimizer optimizer;
    private readonly Stopwatch sessionTimer = new();
    private CancellationTokenSource? cancellationSource;

    private TrainingState? eventBasedState;
    private Task? optimizationTask;

    public TrainingSession(IGeneticOptimizer optimizer, IBackgroundTrainingOrchestrator owningOrchestrator)
    {
        this.optimizer = optimizer;
        OwningOrchestrator = owningOrchestrator;
        this.WhenAnyValue(trainingSession => trainingSession.CurrentGeneration,
                trainingSession => trainingSession.CurrentConfiguration)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ProgressPercentage));
                this.RaisePropertyChanged(nameof(FitnessDiff));
            })
            .DisposeWith(Disposables);
        dispatcherTimer = new DispatcherTimer(TimeSpan.FromSeconds(seconds: 1), DispatcherPriority.Background,
            (_, _) =>
            {
                this.RaisePropertyChanged(nameof(Elapsed));
                this.RaisePropertyChanged(nameof(RemainingMinutes));
            });
        dispatcherTimer.Start();
    }

    public double BestFirstGenerationFitness
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Guid Id
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }
    public string? Name
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public CloudProvider AlertProvider
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public DateTime CreatedAt
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public void Start(OptimizationConfiguration configuration)
    {
        cancellationSource = new CancellationTokenSource();
        sessionTimer.Start();
        dispatcherTimer.Start();
        optimizationTask = Task.Run(() => IterateOptimization(configuration, cancellationSource.Token));
        State = TrainingState.Training;
    }

    public void Stop()
    {
        State = eventBasedState ?? TrainingState.Paused;
        cancellationSource?.Cancel();
        sessionTimer.Stop();
        dispatcherTimer.Stop();
    }

    public double ProgressPercentage => (double)CurrentGeneration / CurrentConfiguration.TotalGenerations;

    public ObservableCollection<double> PopulationDiversity { get; } = [];
    public ObservableCollection<double> AverageGenerationFitness { get; } = [];
    public ObservableCollection<double> BestGenerationFitness { get; } = [];

    public ObservableCollection<AlertScoreCard> TopConfigurations
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = [];

    public int CurrentGeneration
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double BestFitness
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public double FitnessDiff => BestFirstGenerationFitness == 0
        ? 0
        : (BestFitness - BestFirstGenerationFitness) / BestFirstGenerationFitness;

    public int TotalEvaluations
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TimeSpan Elapsed => sessionTimer.Elapsed;

    public double RemainingMinutes
    {
        get
        {
            //bug: count evaluations since recent start and not total evaluations.
            if (TotalEvaluations != 0)
            {
                return Elapsed.TotalMinutes / TotalEvaluations *
                       (CurrentConfiguration.TotalGenerations * CurrentConfiguration.PopulationSize);
            }

            return 0;
        }
    }

    public OptimizationConfiguration CurrentConfiguration
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = OptimizationConfiguration.Default;

    public async Task Hydrate(Guid aggregateId)
    {
        State = TrainingState.Loading;

        await foreach (var @event in optimizer.Hydrate(aggregateId))
        {
            Apply(@event);
        }

        State = eventBasedState ?? TrainingState.Paused;
    }

    public IBackgroundTrainingOrchestrator OwningOrchestrator { get; }

    public ITrainingSession CloneAndStart()
    {
        var newSession = OwningOrchestrator.StartNew(optimizer.Clone());

        return newSession;
    }

    public TrainingState State
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = TrainingState.Paused;

    private void IterateOptimization(OptimizationConfiguration configuration, CancellationToken cancellationToken)
    {
        foreach (var @event in optimizer.Optimize(configuration, cancellationToken))
        {
            Apply(@event);
        }

        Stop();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            dispatcherTimer.Stop();
        }

        base.Dispose(disposing);
    }


    private double GetCurrentPopulationDiversity()
    {
        var diversitySum = 0.0;

        for (var i = 0; i < currentGenerationScores.Count; i++)
        {
            for (var j = 0; j < currentGenerationScores.Count; j++)
            {
                if (i == j)
                {
                    continue;
                }

                var firstConfiguration = currentGenerationScores[i].Configuration;
                var secondConfiguration = currentGenerationScores[j].Configuration;

                diversitySum = firstConfiguration.Distance(secondConfiguration);
            }
        }

        return diversitySum / (currentGenerationScores.Count * (currentGenerationScores.Count - 1));
    }

    private double GetAverageGenerationFitness()
    {
        return currentGenerationScores.Average(score => score.Fitness);
    }

    private double GetBestGenerationFitness()
    {
        return currentGenerationScores.Max(score => score.Fitness);
    }

    private void Apply<T>(T @event) where T : IEvent
    {
        switch (@event)
        {
            case StateInitializedEvent stateInitialized:
                Id = stateInitialized.Id;
                Name = stateInitialized.Name;
                AlertProvider = Enum.Parse<CloudProvider>(stateInitialized.ProviderName);
                CreatedAt = stateInitialized.CreatedAt;
                break;
            case CriticalFailureEvent:
                eventBasedState = TrainingState.Failed;
                break;
            case GenerationCompletedEvent generationCompleted:
                if (generationCompleted is TrainingCompletedEvent)
                {
                    eventBasedState = TrainingState.Completed;
                }

                var currentPopulationDiversity = GetCurrentPopulationDiversity();
                var bestGenerationFitness = GetBestGenerationFitness();
                var averageGenerationFitness = GetAverageGenerationFitness();
                
                Dispatcher.UIThread.Invoke(() =>
                {
                    BestGenerationFitness.Add(bestGenerationFitness);
                    PopulationDiversity.Add(currentPopulationDiversity);
                    AverageGenerationFitness.Add(averageGenerationFitness);
                });
                
                
                if (BestFirstGenerationFitness == 0)
                {
                    BestFirstGenerationFitness = bestGenerationFitness;
                }

                currentGenerationScores.Clear();
                CurrentGeneration += 1;
                break;
            case EvaluationCompletedEvent:
                TotalEvaluations += 1;
                break;
            case AlertScoreComputedEvent scoreComputed:
                currentGenerationScores.Add(scoreComputed.AlertScoreCard);
                UpdateTopConfigurations(scoreComputed.AlertScoreCard);
                BestFitness = Math.Max(BestFitness, scoreComputed.AlertScoreCard.Fitness);
                break;
            case OptimizerConfiguredEvent optimizerConfigured:
                CurrentConfiguration = optimizerConfigured.Configuration;
                break;
        }
    }

    private void UpdateTopConfigurations(AlertScoreCard scoreCard)
    {
        if (scoreCard.Precision > 0.7 || scoreCard.Recall > 0.7 || scoreCard.Fitness > 0.9)
        {
            TopConfigurations.Add(scoreCard);
        }

        if (TopConfigurations.Count > 15)
        {
            TopConfigurations = new ObservableCollection<AlertScoreCard>(TopConfigurations
                .OrderByDescending(card => card.Fitness).Take(count: 10).ToList());
        }
    }
}