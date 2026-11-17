using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Flags in responses, which Moolre sends as <c>1</c>, <c>"1"</c> or <c>true</c>; anything else is <see langword="false"/>.
/// Writes a JSON boolean, which is what the request flags (<c>api</c>, <c>skipotp</c>) expect.
/// </summary>
internal sealed class LenientBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.Number:
                return reader.TryGetInt32(out var number) && number == 1;
            case JsonTokenType.String:
                var text = reader.GetString();
                return string.Equals(text, "1", StringComparison.Ordinal) || string.Equals(text, "true", StringComparison.OrdinalIgnoreCase);
            default:
                reader.Skip();
                return false;
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) => writer.WriteBooleanValue(value);
}
