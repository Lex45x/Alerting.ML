using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Scoring;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Console;

public class TrainingFullSummary : TrainingShortSummary
{
    private HashSet<AlertScoreCard> topTenConfigurations = [];
    public int TotalEvaluations { get; private set; }
    public int GenerationIndex { get; private set; }

    public ReadOnlySet<AlertScoreCard> TopTenConfigurations => topTenConfigurations.AsReadOnly();

    public override void Apply(IEvent @event)
    {
        base.Apply(@event);
        switch (@event)
        {
            case AlertScoreComputedEvent alertScoreComputedEvent:
                TryUpdateTopConfigurations(alertScoreComputedEvent.AlertScoreCard);
                break;
            case EvaluationCompletedEvent _:
                TotalEvaluations += 1;
                break;
            case GenerationCompletedEvent _:
                GenerationIndex += 1;
                break;
        }
    }

    public void TryUpdateTopConfigurations(AlertScoreCard scoreCard)
    {
        if (scoreCard.Precision > 0.7 || scoreCard.Recall > 0.7 || scoreCard.Fitness > 0.9)
        {
            topTenConfigurations.Add(scoreCard);
        }

        if (topTenConfigurations.Count > 10)
        {
            topTenConfigurations = topTenConfigurations.OrderByDescending(card => card.Fitness).Take(10).ToHashSet();
        }
    }

    public override string ToString()
    {
        var summary = new List<string>
        {
            $"{Id} {Name} {Status}"
        };

        foreach (var topTenConfiguration in TopTenConfigurations.OrderByDescending(card => card.Fitness))
        {
            summary.Add($"Fitness: {topTenConfiguration.Fitness} Precision: {topTenConfiguration.Precision:P2} MedianLatency: {topTenConfiguration.MedianDetectionLatency:g} Recall: {topTenConfiguration.Recall:P2} Configuration:");
            summary.Add(JsonSerializer.Serialize(topTenConfiguration.Configuration,
                new JsonSerializerOptions
                    { WriteIndented = true, TypeInfoResolver = KnownTypeInfoResolver.Instance, Converters = { new JsonStringEnumConverter() } }));
        }

        return string.Join(Environment.NewLine, summary);
    }
}