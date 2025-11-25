namespace Alerting.ML.Engine.Optimizer;

public interface IGeneticOptimizer
{
    public IEnumerable<GenerationSummary> Optimize(OptimizationConfiguration configuration);
}

