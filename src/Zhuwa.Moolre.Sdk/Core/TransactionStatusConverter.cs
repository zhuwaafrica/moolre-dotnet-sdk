using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Moolre's <c>txstatus</c>: <c>1</c> successful, <c>2</c> failed, <c>0</c> pending, as a number or text. A missing or unknown value reads
/// as <see cref="TransactionStatus.Pending"/>, never as successful: the SDK must not report money as moved unless Moolre says so.
/// </summary>
internal sealed class TransactionStatusConverter : JsonConverter<TransactionStatus>
{
    public override TransactionStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadInt32(ref reader) switch
        {
            1 => TransactionStatus.Successful,
            2 => TransactionStatus.Failed,
            _ => TransactionStatus.Pending,
        };

    public override void Write(Utf8JsonWriter writer, TransactionStatus value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.ToWire(nameof(value)));
}
