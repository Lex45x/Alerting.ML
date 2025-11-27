namespace Alerting.ML.Engine.Optimizer;

public interface IGeneticOptimizer
{
    public IAsyncEnumerable<GenerationSummary> Optimize(CancellationToken cancellationToken);
}

