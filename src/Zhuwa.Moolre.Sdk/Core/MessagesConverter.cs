using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>Moolre's <c>message</c>, sent as text, an array of text, <c>null</c> or not at all. Every form becomes a list (empty when none).</summary>
internal sealed class MessagesConverter : JsonConverter<List<string>>
{
    public override bool HandleNull => true;

    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var messages = new List<string>();
        if (reader.TokenType == JsonTokenType.String)
        {
            Add(messages, reader.GetString());
        }
        else if (reader.TokenType == JsonTokenType.StartArray)
        {
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.String)
                {
                    Add(messages, reader.GetString());
                }
                else
                {
                    reader.Skip();
                }
            }
        }
        else
        {
            reader.Skip();
        }

        return messages;
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var message in value)
        {
            writer.WriteStringValue(message);
        }

        writer.WriteEndArray();
    }

    private static void Add(List<string> messages, string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            messages.Add(text);
        }
    }
}
