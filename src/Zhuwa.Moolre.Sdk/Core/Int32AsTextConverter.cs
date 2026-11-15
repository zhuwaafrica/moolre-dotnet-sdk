using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>Integers Moolre wants as text (for example the transaction list <c>limit</c>). Reads numbers or text.</summary>
internal sealed class Int32AsTextConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonReading.ReadInt32(ref reader) ?? 0;

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
}
