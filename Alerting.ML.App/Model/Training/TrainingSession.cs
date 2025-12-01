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
using System.Threading;
using System.Threading.Tasks;
using Alerting.ML.App.Model.Enums;
using static System.Collections.Specialized.BitVector32;

namespace Alerting.ML.App.Model.Training;

public class TrainingSession : ViewModelBase, ITrainingSession
{
    private Task? optimizationTask;
    private CancellationTokenSource? cancellationSource;
    private readonly Stopwatch sessionTimer = new Stopwatch();

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
        await foreach (var @event in optimizer.Optimize(configuration, cancellationToken))
        {
            Apply(@event);
        }
    }

    public void Stop()
    {
        cancellationSource?.Cancel();
        sessionTimer.Stop();
    }

    public bool IsPaused => optimizationTask is not { IsCompleted: true };


    private readonly List<AlertScoreCard> currentGenerationScores = new();
    private readonly IGeneticOptimizer optimizer;

    public TrainingSession(IGeneticOptimizer optimizer)
    {
        this.optimizer = optimizer;
        this.WhenAnyValue(trainingSession => trainingSession.CurrentGeneration)
            .Subscribe(_ => { this.RaisePropertyChanged(nameof(ProgressPercentage)); });
    }

    public double ProgressPercentage
    {
        get
        {
            if (CurrentConfiguration != null)
            {
                return ((double)CurrentGeneration) / CurrentConfiguration.TotalGenerations;
            }

            return 0;
        }
    }

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
                BestGenerationFitness.Add(GetBestGenerationFitness());
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

    public ObservableCollection<double> PopulationDiversity { get; } = new();
    public ObservableCollection<double> AverageGenerationFitness { get; } = new();
    public ObservableCollection<double> BestGenerationFitness { get; } = new();
    public ObservableCollection<AlertScoreCard> TopConfigurations { get; } = new();

    public int CurrentGeneration
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    } = 1;

    public double BestFitness
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int TotalEvaluations
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public TimeSpan Elapsed => sessionTimer.Elapsed;

    public OptimizationConfiguration? CurrentConfiguration
    {
        get;
        private set => this.RaiseAndSetIfChanged(ref field, value);
    }
}