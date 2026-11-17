using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>Flags Moolre wants as the text <c>"1"</c> or <c>"0"</c> (for example a payment link's <c>reusable</c>).</summary>
internal sealed class BooleanAsTextConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new LenientBooleanConverter().Read(ref reader, typeToConvert, options);

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteStringValue(value ? "1" : "0");
}
