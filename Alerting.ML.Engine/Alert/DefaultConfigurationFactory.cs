using System.Reflection;

namespace Alerting.ML.Engine.Alert;

/// <summary>
/// todo: reflection here is slow. Generic Type Initializer or Lazy&lt;&gt; can be used to build an expression tree that does the same thing.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultConfigurationFactory<T> : IConfigurationFactory<T>
    where T : AlertConfiguration<T>, new()
{
    private record Parameter(PropertyInfo Property, ConfigurationParameterAttribute Attribute);

    private static readonly IReadOnlyList<Parameter> Parameters = typeof(T)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Select(info =>
            new Parameter(info, info.GetCustomAttributes().OfType<ConfigurationParameterAttribute>().Single()))
        .OrderBy(arg => arg.Attribute.Order)
        .ToList();

    public T Mutate(T value)
    {
        var result = new T();

        foreach (var parameter in Parameters)
        {
            var newValue = parameter.Property.GetValue(value);

            if (Random.Shared.NextDouble() > 0.9)
            {
                newValue = parameter.Attribute.Nudge(newValue, result);
            }

            parameter.Property.SetValue(result, newValue);
        }

        return result;
    }

    public (T, T) Crossover(T first, T second)
    {
        var firstResult = new T();
        var secondResult = new T();

        foreach (var parameter in Parameters)
        {
            parameter.Property.SetValue(firstResult, parameter.Attribute.CrossoverRepair(parameter.Property.GetValue(Random.Shared.NextDouble() > 0.5 ? first : second)!, firstResult));
            parameter.Property.SetValue(secondResult, parameter.Attribute.CrossoverRepair(parameter.Property.GetValue(Random.Shared.NextDouble() > 0.5 ? first : second)!, firstResult));
        }

        return (firstResult, secondResult);
    }

    public T CreateRandom()
    {
        var result = new T();

        foreach (var parameter in Parameters)
        {
            parameter.Property.SetValue(result, parameter.Attribute.GetRandomValue(result));
        }

        return result;
    }
}