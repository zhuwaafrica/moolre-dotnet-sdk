using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// A field Moolre sends as an array of objects, a single object, or an empty object <c>{}</c> meaning "none" (the Payment ID and bank
/// account lookups do all three for <c>transactions</c>). Every form becomes a list.
/// </summary>
/// <typeparam name="T">The entry type, which must be listed in <see cref="MoolreJsonContext"/>.</typeparam>
internal sealed class OneOrManyConverter<T> : JsonConverter<List<T>>
{
    public override bool HandleNull => true;

    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            return new ObjectListConverter<T>().Read(ref reader, typeToConvert, options);
        }

        var items = new List<T>();
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            reader.Skip();
            return items;
        }

        // Read the object as an element first, so the empty object {} can be told apart from a real entry.
        using var document = JsonDocument.ParseValue(ref reader);
        if (document.RootElement.EnumerateObject().Any())
        {
            var itemInfo = (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
            if (document.RootElement.Deserialize(itemInfo) is { } item)
            {
                items.Add(item);
            }
        }

        return items;
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, (JsonTypeInfo<List<T>>)options.GetTypeInfo(typeof(List<T>)));
}
