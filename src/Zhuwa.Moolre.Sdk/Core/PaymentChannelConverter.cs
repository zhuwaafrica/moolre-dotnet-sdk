using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary><see cref="PaymentChannel"/> as Moolre's collection channel code, a number sent as text (<c>"13"</c> MTN, <c>"6"</c>, <c>"7"</c>).</summary>
internal sealed class PaymentChannelConverter : JsonConverter<PaymentChannel>
{
    public override PaymentChannel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadInt32(ref reader) switch
        {
            13 => PaymentChannel.Mtn,
            6 => PaymentChannel.Telecel,
            7 => PaymentChannel.At,
            _ => PaymentChannel.None,
        };

    public override void Write(Utf8JsonWriter writer, PaymentChannel value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToWire(nameof(value)));
}
