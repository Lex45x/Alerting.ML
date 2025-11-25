using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Scoring;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Alerting.ML.Engine.Optimizer;

public class GeneticOptimizer<T> : IGeneticOptimizer
    where T : AlertConfiguration<T>
{
    private readonly IAlert<T> alert;
    private readonly ITimeSeriesProvider timeSeriesProvider;
    private readonly IKnownOutagesProvider knownOutagesProvider;
    private readonly IAlertScoreCalculator alertScoreCalculator;
    private readonly IConfigurationFactory<T> configurationFactory;
    private readonly ILogger<GeneticOptimizer<T>> logger;

    public GeneticOptimizer(IAlert<T> alert, ITimeSeriesProvider timeSeriesProvider,
        IKnownOutagesProvider knownOutagesProvider, IAlertScoreCalculator alertScoreCalculator,
        IConfigurationFactory<T> configurationFactory, ILogger<GeneticOptimizer<T>> logger)
    {
        this.alert = alert;
        this.timeSeriesProvider = timeSeriesProvider;
        this.knownOutagesProvider = knownOutagesProvider;
        this.alertScoreCalculator = alertScoreCalculator;
        this.configurationFactory = configurationFactory;
        this.logger = logger;
    }

    public IEnumerable<GenerationSummary> Optimize(OptimizationConfiguration configuration)
    {
        var generationIndex = -1;
        var stallCount = 0;
        AlertScoreCard? bestScoreCard = null;

        var currentGeneration = Enumerable.Range(0, configuration.PopulationSize)
            .Select(i => configurationFactory.CreateRandom()).ToList();

        logger.LogInformation($"#0 :: Initializing with {configuration.PopulationSize} random entries.");

        while (stallCount < configuration.StallLimit)
        {
            generationIndex++;

            logger.LogInformation($"#{generationIndex} :: Starting evaluation.");

            var stopwatch = Stopwatch.StartNew();

            var generationScore = currentGeneration.AsParallel()
                .Select(alertConfiguration => new
                {
                    Configuration = alertConfiguration,
                    Outages = alert.Evaluate(timeSeriesProvider, alertConfiguration)
                })
                .Select(result =>
                    alertScoreCalculator.CalculateScore(result.Outages, knownOutagesProvider, result.Configuration,
                        configuration.AlertScoreConfiguration))
                .OrderBy(card => card.Score)
                .ToList();

            stopwatch.Stop();

            logger.LogInformation($"#{generationIndex} :: Evaluation completed in {stopwatch.Elapsed:g}");

            var results = generationScore
                .Take(configuration.SurvivorCount)
                .Select(card => card.Configuration as T)
                .ToList();

            logger.LogInformation(
                $"#{generationIndex} :: {configuration.SurvivorCount} survivors preserved for next generation.");

            if (bestScoreCard == null ||
                bestScoreCard.Score > generationScore[0].Score)
            {
                bestScoreCard = generationScore[0];
                stallCount = 0;
            }
            else
            {
                logger.LogInformation($"#{generationIndex} :: Stall for {stallCount} generations.");
                stallCount++;
            }

            logger.LogInformation($"#{generationIndex} :: Best: {generationScore[0]}");
            yield return new GenerationSummary(generationIndex, generationScore, stopwatch.Elapsed);

            var eligibleForTournament = generationScore.Where(card => !card.IsWorst).ToList();

            if (eligibleForTournament.Count < configuration.SurvivorCount)
            {
                logger.LogInformation(
                    $"#{generationIndex} :: Only {eligibleForTournament.Count} entries are eligible for tournament. Adding {configuration.PopulationSize / 2} random entries.");

                for (var i = 0; i < configuration.PopulationSize / 2; i++)
                {
                    results.Add(configurationFactory.CreateRandom());
                }
            }

            logger.LogInformation($"#{generationIndex} :: Starting tournament.");

            while (results.Count < configuration.PopulationSize)
            {
                var first = Tournament(configuration,
                    eligibleForTournament.Count >= configuration.SurvivorCount
                        ? eligibleForTournament
                        : generationScore);
                var second = Tournament(configuration,
                    eligibleForTournament.Count >= configuration.SurvivorCount
                        ? eligibleForTournament
                        : generationScore);

                if (Random.Shared.NextDouble() > configuration.CrossoverProbability)
                {
                    (first, second) = configurationFactory.Crossover(first, second);
                }

                first = configurationFactory.Mutate(first);
                second = configurationFactory.Mutate(second);

                results.Add(first);
                results.Add(second);
            }

            logger.LogInformation($"#{generationIndex} :: Tournament completed.");


            logger.LogInformation(
                $"#{generationIndex} :: Generation completed. Next generation size is {results.Count}");

            currentGeneration = results;
        }
    }

    private static T Tournament(OptimizationConfiguration configuration, IReadOnlyList<AlertScoreCard> generationScore)
    {
        var winner = generationScore[Random.Shared.Next(0, generationScore.Count)];

        for (var i = 0; i < configuration.TournamentsCount; i++)
        {
            var challenger = generationScore[Random.Shared.Next(0, generationScore.Count)];
            if (challenger.Score > winner.Score)
            {
                winner = challenger;
            }
        }

        return (T)winner.Configuration;
    }
}

public class GenerationSummary
{
    public int GenerationIndex { get; }
    public TimeSpan SimulationDuration { get; }

    public GenerationSummary(int generationIndex, IReadOnlyList<AlertScoreCard> generation, TimeSpan simulationDuration)
    {
        GenerationIndex = generationIndex;
        SimulationDuration = simulationDuration;
        Best = generation[0];
        PrecisionDistribution = generation
            .BinBy(card => card.Precision, 0.1, 2)
            .ToList();
        FalseNegativeRateDistribution = generation
            .BinBy(card => card.FalseNegativeRate, 0.1, 2)
            .ToList();
        DetectionLatencyDistribution = generation
            .BinBy(card => card.MedianDetectionLatency.Ticks, TimeSpan.FromMinutes(1).Ticks)
            .Select(tuple => (tuple.Count, TimeSpan.FromTicks((long)tuple.Value)))
            .ToList();
        ScoreDistribution = generation
            .BinBy(card => card.Score, 100)
            .ToList();
        OutageCountDistribution = generation
            .BinBy(card => card.OutagesCount, 1)
            .ToList();
    }

    public IReadOnlyList<(int Count, double Value)> ScoreDistribution { get; }

    public IReadOnlyList<(int Count, TimeSpan Value)> DetectionLatencyDistribution { get; }

    public IReadOnlyList<(int Count, double Value)> FalseNegativeRateDistribution { get; }

    public IReadOnlyList<(int Count, double Value)> PrecisionDistribution { get; }
    public IReadOnlyList<(int Count, double Value)> OutageCountDistribution { get; }

    public AlertScoreCard Best { get; }
}

internal static class EnumerableExtensions
{
    public static IEnumerable<(int Count, double Value)> BinBy<T>(this IEnumerable<T> input, Func<T, double> selector,
        double step, int roundTo = 0)
    {
        return input.Select(selector).GroupBy(value => Math.Round(value / step, roundTo))
            .Select(doubles => (doubles.Count(), doubles.Key * step));
    }
}