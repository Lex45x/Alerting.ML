using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Alerting.ML.Engine.Storage;

internal class EventTypeInfoResolver : DefaultJsonTypeInfoResolver
{
    private static readonly IReadOnlyList<Type> KnownEventTypes;

    static EventTypeInfoResolver()
    {
        KnownEventTypes = typeof(IEvent).Assembly.GetTypes()
            .Where(type =>
                type.IsAssignableTo(typeof(IEvent)) && type is { IsAbstract: false, IsGenericTypeDefinition: false })
            .ToList();
    }

    public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        var typeInfo = base.GetTypeInfo(type, options);

        var baseEventType = typeof(IEvent);

        if (typeInfo.Type != baseEventType)
        {
            return typeInfo;
        }

        typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
        {
            TypeDiscriminatorPropertyName = "$event-type",
            IgnoreUnrecognizedTypeDiscriminators = true,
            UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization
        };

        foreach (var knownEventType in KnownEventTypes)
        {
            typeInfo.PolymorphismOptions.DerivedTypes.Add(new JsonDerivedType(knownEventType, knownEventType.Name));
        }

        return typeInfo;
    }
}