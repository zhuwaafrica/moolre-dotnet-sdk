using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// A nested object in a response that Moolre sometimes sends as something else (for example <c>account: null</c> when not found).
/// Anything that is not an object reads as <see langword="null"/> instead of failing the whole result.
/// </summary>
/// <typeparam name="T">The object type, which must be listed in <see cref="MoolreJsonContext"/>.</typeparam>
internal sealed class LenientObjectConverter<T> : JsonConverter<T>
    where T : class
{
    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            reader.Skip();
            return null;
        }

        // The type's own contract from the source-generated context (it has no converter of its own, so there is no recursion).
        return JsonSerializer.Deserialize(ref reader, (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T)));
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T)));
}
