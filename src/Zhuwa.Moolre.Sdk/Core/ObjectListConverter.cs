using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// A list of objects in a response. Entries that are not objects are skipped, and a value that is not an array reads as an empty list,
/// so one odd entry never hides the others or fails the result.
/// </summary>
/// <typeparam name="T">The entry type, which must be listed in <see cref="MoolreJsonContext"/>.</typeparam>
internal sealed class ObjectListConverter<T> : JsonConverter<List<T>>
{
    public override bool HandleNull => true;

    public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var items = new List<T>();
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            reader.Skip();
            return items;
        }

        // The entry contract comes from the source-generated context, so no reflection is used (trimming and AOT safe).
        var itemInfo = (JsonTypeInfo<T>)options.GetTypeInfo(typeof(T));
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType == JsonTokenType.StartObject && JsonSerializer.Deserialize(ref reader, itemInfo) is { } item)
            {
                items.Add(item);
            }
            else
            {
                reader.Skip();
            }
        }

        return items;
    }

    public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value, (JsonTypeInfo<List<T>>)options.GetTypeInfo(typeof(List<T>)));
}
