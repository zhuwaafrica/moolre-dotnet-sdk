using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Decimals in responses, which Moolre sends as numbers (<c>10.67</c>) or text (<c>"1.01"</c>); anything else reads as 0.
/// Writes a JSON number (the one request field that wants a number, the bank account amount, uses this).
/// </summary>
internal sealed class LenientDecimalConverter : JsonConverter<decimal>
{
    public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadDecimal(ref reader) ?? 0m;

    public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options) => writer.WriteNumberValue(value);
}
