using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Money amounts Moolre wants as text, for example <c>"10.5"</c>. Always the invariant culture, so a culture that writes a decimal
/// comma can never change the amount. Reads numbers or text.
/// </summary>
internal sealed class DecimalAsTextConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadDecimal(ref reader) ?? 0m;

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToWire());
}
