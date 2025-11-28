using Alerting.ML.Engine.Optimizer;

namespace Alerting.ML.Engine.Alert;

/// <summary>
/// This interface defines a list of actions that will be performed on an <see cref="AlertConfiguration"/> during training process.
/// Exists purely for extensibility purposes if <see cref="DefaultConfigurationFactory{T}"/> will not be enough.
/// Implementation of descendants must take into account <see cref="OptimizationConfiguration"/>.
/// </summary>
/// <typeparam name="TConfiguration">Type of AlertConfiguration</typeparam>
public interface IConfigurationFactory<TConfiguration> : IConfigurationFactory where TConfiguration : AlertConfiguration
{
    /// <summary>
    /// Slightly changes values of each <typeparamref name="TConfiguration"/>'s property with probability of <paramref name="mutationProbability"/>
    /// </summary>
    /// <param name="value">Instance of alert configuration.</param>
    /// <param name="mutationProbability">Probability of mutation from 0 to 1.</param>
    /// <returns>Instance of alert configuration with randomly <see cref="ConfigurationParameterAttribute.Nudge"/>'d properties. </returns>
    TConfiguration Mutate(TConfiguration value, double mutationProbability);

    /// <summary>
    /// Exchanges values of the same properties between two configuration instances with equal probability.
    /// </summary>
    /// <param name="first">Instance of alert configuration.</param>
    /// <param name="second">Instance of alert configuration.</param>
    /// <returns>Instances with recombined properties.</returns>
    (TConfiguration, TConfiguration) Crossover(TConfiguration first, TConfiguration second);

    /// <summary>
    /// Creates a new instance of <typeparamref name="TConfiguration"/> with random property values.
    /// </summary>
    /// <returns></returns>
    TConfiguration CreateRandom();
}

/// <summary>
/// Non-generic version of the interface for a limited type-safety outside Generic context.
/// </summary>
public interface IConfigurationFactory
{
}