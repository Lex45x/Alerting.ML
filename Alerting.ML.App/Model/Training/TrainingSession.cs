using Alerting.ML.App.ViewModels;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Threading;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using Avalonia.Threading;

namespace Alerting.ML.App.Model.Training;

public class TrainingSession : ViewModelBase, ITrainingSession
{
    private Task? optimizationTask;
    private CancellationTokenSource? cancellationSource;
    private readonly Stopwatch sessionTimer = new Stopwatch();
    private readonly DispatcherTimer timer;

    public Guid Id => optimizer.Id;

    //todo: how to generate a neat name?
    public string? Name { get; }


    public void Start(OptimizationConfiguration configuration)
    {
        cancellationSource = new CancellationTokenSource();
        sessionTimer.Start();
        optimizationTask = Task.Run(() => IterateOptimization(configuration, cancellationSource.Token));
    }

    private async Task IterateOptimization(OptimizationConfiguration configuration, CancellationToken cancellationToken)
    {
        IsPaused = false;

        await foreach (var @event in optimizer.Optimize(configuration, cancellationToken))
        {
            Apply(@event);
        }

        IsPaused = true;
    }

    public void Stop()
    {
        cancellationSource?.Cancel();
        sessionTimer.Stop();
    }

    public bool IsPaused
    {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    } = true;


    private readonly List<AlertScoreCard> currentGenerationScores = [];
    private readonly IGeneticOptimizer optimizer;

    public TrainingSession(IGeneticOptimizer optimizer)
    {
        this.optimizer = optimizer;
        this.WhenAnyValue(trainingSession => trainingSession.CurrentGeneration,
                trainingSession => trainingSession.CurrentConfiguration)
            .Subscribe(_ =>
            {
                this.RaisePropertyChanged(nameof(ProgressPercentage));
                this.RaisePropertyChanged(nameof(FitnessDiff));
            })
            .DisposeWith(Disposables);
        timer = new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.Background,
            (_, _) =>
            {
                this.RaisePropertyChanged(nameof(Elapsed));
                this.RaisePropertyChanged(nameof(RemainingMinutes));
            });
        timer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            timer.Stop();
        }

        base.Dispose(disposing);
    }

    public double ProgressPercentage => (double)CurrentGeneration / CurrentConfiguration.TotalGenerations;

    //todo: this value has to be initialized *somehow* during TrainingBuilder configuration.
    public CloudProvider AlertProvider { get; } = CloudProvider.Azure;

    //todo: this value has to be initialized *somehow* during TrainingBuilder configuration.
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

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

    private double GetAverageGenerationFitness() => currentGenerationScores.Average(score => score.Fitness);
    private double GetBestGenerationFitness() => currentGenerationScores.Max(score => score.Fitness);

    private void Apply<T>(T @event) where T : IEvent
    {
        switch (@event)
        {
            case GenerationCompletedEvent:
                PopulationDiversity.Add(GetCurrentPopulationDiversity());
                AverageGenerationFitness.Add(GetAverageGenerationFitness());
                var bestGenerationFitness = GetBestGenerationFitness();
                BestGenerationFitness.Add(bestGenerationFitness);
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
                BestFitness = Math.Max(BestFitness, scoreComputed.AlertScoreCard.Fitness);
                break;
            case OptimizerConfiguredEvent optimizerConfigured:
                CurrentConfiguration = optimizerConfigured.Configuration;
                break;
        }
    }

    public ObservableCollection<double> PopulationDiversity { get; } = [];
    public ObservableCollection<double> AverageGenerationFitness { get; } = [];
    public ObservableCollection<double> BestGenerationFitness { get; } = [];
    public ObservableCollection<AlertScoreCard> TopConfigurations { get; } = [];

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

    public double BestFirstGenerationFitness
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
            if (TotalEvaluations != 0)
            {
                return (Elapsed.TotalMinutes / TotalEvaluations) *
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
}