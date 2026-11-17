using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// <see cref="SalesNetwork"/> as Moolre's network name (<c>MTN</c>, <c>TELECEL</c>, <c>AT</c>). Reading also accepts the legacy codes
/// <c>1</c>, <c>6</c>, <c>7</c>, as text or a number, which Moolre still sends in places.
/// </summary>
internal sealed class SalesNetworkConverter : JsonConverter<SalesNetwork>
{
    public override SalesNetwork Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        new MoolreStringConverter().Read(ref reader, typeof(string), options)?.ToUpperInvariant() switch
        {
            "MTN" or "1" => SalesNetwork.Mtn,
            "TELECEL" or "6" => SalesNetwork.Telecel,
            "AT" or "7" => SalesNetwork.At,
            _ => SalesNetwork.None,
        };

    public override void Write(Utf8JsonWriter writer, SalesNetwork value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToWire(nameof(value)));
}
