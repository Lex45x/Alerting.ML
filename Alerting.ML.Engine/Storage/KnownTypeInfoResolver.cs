using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Alerting.ML.Engine.Alert;
using Alerting.ML.Engine.Scoring;

namespace Alerting.ML.Engine.Storage;

/// <summary>
///     Informs <see cref="JsonSerializer" /> about hierarchical data types and allows extension with additional types.
/// </summary>
public class KnownTypeInfoResolver : DefaultJsonTypeInfoResolver, IConfigurationTypeRegistry
{
    private static readonly ConcurrentDictionary<Type, HashSet<Type>> KnownTypes = new()
    {
        [typeof(IAlertScoreCalculator)] = [typeof(DefaultAlertScoreCalculator)],
        [typeof(IConfigurationFactory)] = [typeof(DefaultConfigurationFactory<>)],
        [typeof(IEvent)] = typeof(IEvent).Assembly.GetTypes()
            .Where(type => type.IsAssignableTo(typeof(IEvent)) && type is { IsAbstract: false })
            .ToHashSet()
    };

    /// <summary>
    ///     Represent a global registry of configuration types.
    /// </summary>
    public static KnownTypeInfoResolver Instance { get; } = new();

    /// <inheritdoc />
    public void RegisterConfigurationType<T>() where T : AlertConfiguration
    {
        KnownTypes.GetOrAdd(typeof(AlertConfiguration), _ => []).Add(typeof(T));
    }

    /// <inheritdoc />
    public void RegisterAlertType<T>() where T : IAlert
    {
        RegisterAlertType(typeof(T));
    }

    /// <inheritdoc />
    public void RegisterAlertType(Type alertType)
    {
        KnownTypes.GetOrAdd(typeof(IAlert), _ => []).Add(alertType);
    }

    /// <inheritdoc />
    public void RegisterConfigurationFactoryType(Type factoryType)
    {
        KnownTypes.GetOrAdd(typeof(IConfigurationFactory), _ => []).Add(factoryType);
    }

    /// <inheritdoc />
    public void RegisterScoreCalculatorType<T>() where T : IAlertScoreCalculator
    {
        KnownTypes.GetOrAdd(typeof(IAlertScoreCalculator), _ => []).Add(typeof(T));
    }


    /// <inheritdoc />
    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        if (typeInfo.Type == typeof(IEvent))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$event-type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var knownEventType in KnownTypes[typeof(IEvent)])
            {
                if (knownEventType.IsGenericTypeDefinition)
                {
                    foreach (var knownConfigurationType in KnownTypes[typeof(AlertConfiguration)])
                    {
                        typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(
                            knownEventType.MakeGenericType(knownConfigurationType),
                            $"{knownEventType.Name}|{knownConfigurationType.Name}"));
                    }
                }
                else
                {
                    typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(knownEventType,
                        knownEventType.Name));
                }
            }
        }
        else if (typeInfo.Type == typeof(AlertConfiguration))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$configuration-type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var knownConfigurationType in KnownTypes[typeof(AlertConfiguration)])
            {
                typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(knownConfigurationType,
                    knownConfigurationType.Name));
            }
        }
        else if (typeInfo.Type == typeof(IConfigurationFactory) || (typeInfo.Type.IsConstructedGenericType &&
                                                                    typeInfo.Type.GetGenericTypeDefinition() ==
                                                                    typeof(IConfigurationFactory<>)))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$factory-type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var knownFactoryType in KnownTypes[typeof(IConfigurationFactory)])
            {
                foreach (var knownConfigurationType in KnownTypes[typeof(AlertConfiguration)])
                {
                    typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(
                        knownFactoryType.MakeGenericType(knownConfigurationType),
                        $"{knownFactoryType.Name}|{knownConfigurationType.Name}"));
                }
            }
        }
        else if (typeInfo.Type == typeof(IAlert) || (typeInfo.Type.IsConstructedGenericType &&
                                                     typeInfo.Type.GetGenericTypeDefinition() == typeof(IAlert<>)))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$alert-type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var knownAlertType in KnownTypes[typeof(IAlert)])
            {
                typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(knownAlertType, knownAlertType.Name));
            }
        }
        else if (typeInfo.Type == typeof(IAlertScoreCalculator))
        {
            typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
            {
                TypeDiscriminatorPropertyName = "$calculator-type",
                IgnoreUnrecognizedTypeDiscriminators = true,
                UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
            };

            foreach (var knownCalculatorType in KnownTypes[typeof(IAlertScoreCalculator)])
            {
                typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(knownCalculatorType,
                    knownCalculatorType.Name));
            }
        }

        return typeInfo;
    }
}

/// <summary>
///     Configures polymorphic serialization of events by supplying descendants of known types in runtime.
/// </summary>
public interface IConfigurationTypeRegistry
{
    /// <summary>
    ///     Registers known configuration type.
    /// </summary>
    /// <typeparam name="T">Exact configuration type.</typeparam>
    void RegisterConfigurationType<T>() where T : AlertConfiguration;

    /// <summary>
    ///     Registers known alert type.
    /// </summary>
    /// <typeparam name="T">Exact alert type.</typeparam>
    void RegisterAlertType<T>() where T : IAlert;

    /// <summary>
    ///     Registers known alert type outside generic context.
    /// </summary>
    /// <param name="alertType"></param>
    void RegisterAlertType(Type alertType);

    /// <summary>
    ///     Registers known AlertScoreCalculator type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void RegisterScoreCalculatorType<T>() where T : IAlertScoreCalculator;

    /// <summary>
    ///     Registers open generic configuration factory type.
    /// </summary>
    /// <param name="factoryType"></param>
    void RegisterConfigurationFactoryType(Type factoryType);
}