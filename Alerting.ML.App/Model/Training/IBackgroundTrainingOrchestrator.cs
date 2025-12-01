using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Alerting.ML.Engine.Optimizer;

namespace Alerting.ML.App.Model.Training;

public interface IBackgroundTrainingOrchestrator
{
    ITrainingSession StartNew(IGeneticOptimizer optimizer);
    ObservableCollection<ITrainingSession> AllSessions { get; }
}