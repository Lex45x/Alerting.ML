using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Storage;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// Implements a genetic algorithm to optimize <see cref="AlertConfiguration"/>.
/// </summary>
public interface IGeneticOptimizer
{
    /// <summary>
    /// Whenever new optimizer is created, a new Id is associated with it.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Every time a change to generation state happens, yields an <see cref="IEvent"/> associated with the change.
    /// When cancelled and called again - should pick up the process where it was left.
    /// </summary>
    /// <param name="configuration">A configuration to apply to current run.</param>
    /// <param name="cancellationToken">Allows to pause optimization run.</param>
    /// <returns>A stream of events generated during the simulation.</returns>
    public IAsyncEnumerable<IEvent> Optimize(OptimizationConfiguration configuration, CancellationToken cancellationToken);
}