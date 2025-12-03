using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;

namespace Alerting.ML.App.Model.Training;

public interface IBackgroundTrainingOrchestrator
{
    TrainingBuilder DefaultBuilder { get; }
    ObservableCollection<ITrainingSession> AllSessions { get; }
    ITrainingSession StartNew(IGeneticOptimizer optimizer);
    Task ImportFromEventStore();
}