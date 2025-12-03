using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.App.Model.Training;

public interface IBackgroundTrainingOrchestrator
{
    TrainingBuilder DefaultBuilder { get; }
    ITrainingSession StartNew(IGeneticOptimizer optimizer);
    Task ImportFromEventStore();
    ObservableCollection<ITrainingSession> AllSessions { get; }
}