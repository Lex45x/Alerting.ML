using Alerting.ML.Engine.Optimizer;
using DynamicData;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Alerting.ML.App.Model.Training;

public class BackgroundTrainingOrchestrator : IBackgroundTrainingOrchestrator
{
    public ITrainingSession StartNew(IGeneticOptimizer optimizer)
    {
        var trainingSession = new TrainingSession(optimizer);

        AllSessions.Add(trainingSession);
        
        trainingSession.Start(OptimizationConfiguration.Default);

        return trainingSession;
    }

    //todo: this should be read from application state.
    public ObservableCollection<ITrainingSession> AllSessions { get; } = new();
}