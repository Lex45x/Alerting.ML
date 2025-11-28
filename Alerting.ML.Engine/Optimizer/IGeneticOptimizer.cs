using Alerting.ML.Engine.Alert;

namespace Alerting.ML.Engine.Optimizer;

/// <summary>
/// Implements a genetic algorithm to optimize <see cref="AlertConfiguration"/>.
/// </summary>
public interface IGeneticOptimizer
{
    /// <summary>
    /// Every time a generation evaluation is completed, yields a new instance of <see cref="GenerationSummary"/>.
    /// When cancelled and called again - should pick up the process where it was left.
    /// </summary>
    /// <param name="cancellationToken">Allows to pause optimization run.</param>
    /// <returns></returns>
    public IAsyncEnumerable<GenerationSummary> Optimize(CancellationToken cancellationToken);
}

