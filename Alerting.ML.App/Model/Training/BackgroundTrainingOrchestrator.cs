using Alerting.ML.Engine.Optimizer;
using DynamicData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Alerting.ML.App.Model.Training;

public class BackgroundTrainingOrchestrator : IBackgroundTrainingOrchestrator
{
    //todo: this should be read from application state.
    private readonly ConcurrentDictionary<Guid, ITrainingSession> allSessions = new();

    public ITrainingSession StartNew(IGeneticOptimizer optimizer)
    {
        var trainingSession = new TrainingSession(optimizer);

        if (!allSessions.TryAdd(trainingSession.Id, trainingSession))
        {
            throw new InvalidOperationException($"Can't start session with Id {optimizer.Id} as it's already started!");
        }
        
        trainingSession.Start(OptimizationConfiguration.Default);

        return trainingSession;
    }

    public IReadOnlyDictionary<Guid, ITrainingSession> AllSessions => allSessions;
}