using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>A sale status lookup's <c>idtype</c>: <c>1</c> external reference, <c>2</c> transaction ID, <c>3</c> sale reference.</summary>
internal sealed class SaleIdKindConverter : JsonConverter<SaleIdKind>
{
    public override SaleIdKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadInt32(ref reader) switch
        {
            1 => SaleIdKind.ExternalReference,
            2 => SaleIdKind.TransactionId,
            3 => SaleIdKind.SaleReference,
            _ => SaleIdKind.None,
        };

    public override void Write(Utf8JsonWriter writer, SaleIdKind value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.ToWire(nameof(value)));
}
