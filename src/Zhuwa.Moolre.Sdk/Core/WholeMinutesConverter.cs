using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>A duration Moolre wants in whole minutes (a payment link's <c>expiration_time</c>). The client checks it is whole minutes first.</summary>
internal sealed class WholeMinutesConverter : JsonConverter<TimeSpan>
{
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        TimeSpan.FromMinutes(JsonReading.ReadInt32(ref reader) ?? 0);

    public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options) =>
        writer.WriteNumberValue((int)value.TotalMinutes);
}
