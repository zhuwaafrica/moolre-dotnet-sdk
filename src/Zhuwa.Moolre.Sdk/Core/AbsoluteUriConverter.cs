using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>URLs. Writes the absolute form; reads only an absolute URL, so Moolre's empty <c>""</c> callback becomes <see langword="null"/>.</summary>
internal sealed class AbsoluteUriConverter : JsonConverter<Uri>
{
    public override Uri? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            reader.Skip();
            return null;
        }

        return Uri.TryCreate(reader.GetString(), UriKind.Absolute, out var uri) ? uri : null;
    }

    public override void Write(Utf8JsonWriter writer, Uri value, JsonSerializerOptions options) => writer.WriteStringValue(value.AbsoluteUri);
}
