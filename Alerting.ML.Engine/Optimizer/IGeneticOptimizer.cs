using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
///     Implements a genetic algorithm to optimize <see cref="AlertConfiguration" />.
/// </summary>
public interface IGeneticOptimizer
{
    /// <summary>
    ///     The Id of optimization.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    ///     Friendly name of the optimization.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Friendly name of the alert provider used in this optimizer.
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    ///     DateTime when optimization was created.
    /// </summary>
    public DateTime CreatedAt { get; }

    /// <summary>
    ///     Initializes current instance of optimizer from event store.
    /// </summary>
    /// <param name="aggregateId"></param>
    /// <returns></returns>
    public IAsyncEnumerable<IEvent> Hydrate(Guid aggregateId);

    /// <summary>
    ///     Every time a change to generation state happens, yields an <see cref="IEvent" /> associated with the change.
    ///     When cancelled and called again - should pick up the process where it was left.
    /// </summary>
    /// <param name="configuration">A configuration to apply to current run.</param>
    /// <param name="cancellationToken">Allows to pause optimization run.</param>
    /// <returns>A stream of events generated during the simulation.</returns>
    public IEnumerable<IEvent> Optimize(OptimizationConfiguration configuration, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new genetic optimizer instance with a copy of current optimizer configuration.
    /// </summary>
    /// <returns></returns>
    IGeneticOptimizer Clone();
}