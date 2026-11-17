using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary><see cref="Currency"/> as Moolre's code, <c>GHS</c> or <c>NGN</c>.</summary>
internal sealed class CurrencyConverter : JsonConverter<Currency>
{
    public override Currency Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new MoolreStringConverter().Read(ref reader, typeof(string), options) switch
        {
            "GHS" => Currency.Ghs,
            "NGN" => Currency.Ngn,
            _ => Currency.None,
        };

    public override void Write(Utf8JsonWriter writer, Currency value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToWire(nameof(value)));
}
