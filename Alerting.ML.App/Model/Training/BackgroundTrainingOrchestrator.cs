using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Alerting.ML.Engine;
using Alerting.ML.Engine.Optimizer;
using Alerting.ML.Engine.Optimizer.Events;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.App.Model.Training;

public class BackgroundTrainingOrchestrator : IBackgroundTrainingOrchestrator
{
    private readonly IEventStore eventStore;

    public BackgroundTrainingOrchestrator(IEventStore eventStore)
    {
        this.eventStore = eventStore;
        DefaultBuilder = TrainingBuilder.Create().WithCustomEventStore(this.eventStore);
    }

    public TrainingBuilder DefaultBuilder { get; }

    public ITrainingSession StartNew(IGeneticOptimizer optimizer)
    {
        var trainingSession = new TrainingSession(optimizer, this);

        AllSessions.Add(trainingSession);

        trainingSession.Start(OptimizationConfiguration.Default);

        return trainingSession;
    }

    public async Task ImportFromEventStore()
    {
        var hydrationTasks = new List<Task>();
        await foreach (var aggregateId in eventStore.GetExistingAggregates())
        {
            var initializationEvent = await eventStore.GetAll(aggregateId, CancellationToken.None).FirstAsync();

            if (initializationEvent.GetType().GetGenericTypeDefinition() != typeof(StateInitializedEvent<>))
            {
                //todo: this aggregate's events sequence appears to be broken adn can't be serialized. Proper error handling here is necessary.
                continue;
            }

            var configurationType = initializationEvent.GetType().GetGenericArguments()[0];

            var geneticOptimizer = DefaultBuilder.CreateEmpty(configurationType);

            var trainingSession = new TrainingSession(geneticOptimizer, this);

            AllSessions.Add(trainingSession);

            hydrationTasks.Add(Task.Run(() => trainingSession.Hydrate(aggregateId)));
        }

        await Task.WhenAll(hydrationTasks);
    }

    //todo: this should be read from application state.
    public ObservableCollection<ITrainingSession> AllSessions { get; } = new();
}