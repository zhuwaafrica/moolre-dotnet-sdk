using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary><see cref="TransferChannel"/> as Moolre's transfer channel code, a number sent as text (<c>"1"</c> MTN, <c>"2"</c> bank, ...).</summary>
internal sealed class TransferChannelConverter : JsonConverter<TransferChannel>
{
    public override TransferChannel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadInt32(ref reader) switch
        {
            1 => TransferChannel.Mtn,
            2 => TransferChannel.Bank,
            6 => TransferChannel.Telecel,
            7 => TransferChannel.At,
            _ => TransferChannel.None,
        };

    public override void Write(Utf8JsonWriter writer, TransferChannel value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToWire(nameof(value)));
}
