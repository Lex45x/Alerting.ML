using System;
using System.Collections.Generic;
using Alerting.ML.Engine.Optimizer;

namespace Alerting.ML.App.Model.Training;

public interface IBackgroundTrainingOrchestrator
{
    ITrainingSession StartNew(IGeneticOptimizer optimizer);
    IReadOnlyDictionary<Guid, ITrainingSession> AllSessions { get; }
}