using System.Text.Json;
using System.Text.Json.Serialization;

namespace Plates.Engine;

/// <summary>Writes a <see cref="Weight"/> as a plain number of kg, so scheme JSON reads <c>"increment": 5</c>.</summary>
public sealed class WeightJsonConverter : JsonConverter<Weight>
{
    public override Weight Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Weight.FromKg(reader.GetDecimal());

    public override void Write(Utf8JsonWriter writer, Weight value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Kg);
}
