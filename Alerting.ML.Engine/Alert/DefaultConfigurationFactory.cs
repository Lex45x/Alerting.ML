using System.Reflection;

namespace Alerting.ML.Engine.Alert;

/// <summary>
///     A default implementation of configuration factory. Actions available on configuration parameters are defined by
///     <see cref="ConfigurationParameterAttribute" />.
/// </summary>
/// <typeparam name="T">Type of AlertConfiguration</typeparam>
public class DefaultConfigurationFactory<T> : IConfigurationFactory<T>
    where T : AlertConfiguration, new()
{
    private const double FiftyPercentProbability = 0.5;

    //todo: reflection here is slow. Expression tree that does the same thing can be built instead.
    private static readonly IReadOnlyList<Parameter> Parameters = typeof(T)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Select(info =>
            new Parameter(info, info.GetCustomAttributes().OfType<ConfigurationParameterAttribute>().Single()))
        .OrderBy(arg => arg.Attribute.Order)
        .ToList();


    /// <inheritdoc />
    public T Mutate(T value, double mutationProbability)
    {
        var result = new T();

        foreach (var parameter in Parameters)
        {
            var newValue = parameter.Property.GetValue(value)!;

            if (Random.Shared.NextDouble() > 1 - mutationProbability)
            {
                newValue = parameter.Attribute.Nudge(newValue, result);
            }

            parameter.Property.SetValue(result, newValue);
        }

        return result;
    }

    /// <inheritdoc />
    public (T, T) Crossover(T first, T second)
    {
        var firstResult = new T();
        var secondResult = new T();

        foreach (var parameter in Parameters)
        {
            parameter.Property.SetValue(firstResult,
                parameter.Attribute.CrossoverRepair(
                    parameter.Property.GetValue(Random.Shared.NextDouble() > FiftyPercentProbability ? first : second)!,
                    firstResult));
            parameter.Property.SetValue(secondResult,
                parameter.Attribute.CrossoverRepair(
                    parameter.Property.GetValue(Random.Shared.NextDouble() > FiftyPercentProbability ? first : second)!,
                    firstResult));
        }

        return (firstResult, secondResult);
    }

    /// <inheritdoc />
    public T CreateRandom()
    {
        var result = new T();

        foreach (var parameter in Parameters)
        {
            parameter.Property.SetValue(result, parameter.Attribute.GetRandomValue(result));
        }

        return result;
    }

    private record Parameter(PropertyInfo Property, ConfigurationParameterAttribute Attribute);
}