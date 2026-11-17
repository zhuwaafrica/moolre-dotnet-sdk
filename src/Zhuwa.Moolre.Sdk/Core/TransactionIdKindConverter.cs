using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>A status lookup's <c>idtype</c>: <c>1</c> external reference, <c>2</c> Moolre ID.</summary>
internal sealed class TransactionIdKindConverter : JsonConverter<TransactionIdKind>
{
    public override TransactionIdKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadInt32(ref reader) switch
        {
            1 => TransactionIdKind.ExternalReference,
            2 => TransactionIdKind.MoolreId,
            _ => TransactionIdKind.None,
        };

    public override void Write(Utf8JsonWriter writer, TransactionIdKind value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.ToWire(nameof(value)));
}
