using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alerting.ML.Engine.Data;
using Alerting.ML.Engine.Optimizer.Events;

namespace Alerting.ML.Engine.Storage;

/// <summary>
///     Optimized JSON Serialization for <see cref="ImmutableArray{T}" /> of <see cref="Metric" />.
///     Used for <see cref="StateInitializedEvent{T}" /> serialization and deserialization. <br />
///     Instead of writing plain array, creates an object with recorded Length property for allocating result array of a
///     given size.<br />
///     Each <see cref="Metric" /> struct is serialized as two sequential numbers: <see cref="Metric.Timestamp" />.Ticks
///     and <see cref="Metric.Value" /> allowing to avoid excessive strings parsing.<br />
///     Resulting json looks like:
///     <code>
/// {
///   "Length": 2,
///   "Values": [ticks, value, ticks, value]
/// }
/// </code>
/// </summary>
public class MetricsListConverter : JsonConverter<ImmutableArray<Metric>>
{
    /// <inheritdoc />
    public override ImmutableArray<Metric> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expecting a start of object");
        }

        reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Length")
        {
            throw new JsonException("Expecting property Length");
        }

        reader.Read();

        var length = reader.GetInt32();

        var result = new Metric[length];

        reader.Read();

        if (reader.TokenType != JsonTokenType.PropertyName || reader.GetString() != "Values")
        {
            throw new JsonException("Expecting property Values");
        }

        reader.Read();

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expecting Values to be an array");
        }

        var arrayIndex = 0;

        // from now on we are reading array of numbers where every even number is DateTime.Ticks and every odd is Value of the metric.
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            var ticks = reader.GetInt64();
            reader.Read();
            var value = reader.GetDouble();

            result[arrayIndex++] = new Metric(new DateTime(ticks), value);
        }

        reader.Read();

        return arrayIndex != length
            ? throw new JsonException("Invalid length supplied for Metrics array!")
            : [.. result];
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, ImmutableArray<Metric> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteNumber("Length", value.Length);
        writer.WriteStartArray("Values");
        for (var i = 0; i < value.Length; i++)
        {
            writer.WriteNumberValue(value[i].Timestamp.Ticks);
            writer.WriteNumberValue(value[i].Value);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}