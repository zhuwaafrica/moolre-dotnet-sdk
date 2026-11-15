using System.Text.Json;
using System.Text.Json.Serialization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Text that Moolre wants present even when unset, as an empty string (the settlement fields, where empty means "not configured").
/// Reading turns an empty value back into <see langword="null"/>.
/// </summary>
internal sealed class EmptyWhenNullStringConverter : JsonConverter<string?>
{
    public override bool HandleNull => true;

    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = new MoolreStringConverter().Read(ref reader, typeof(string), options);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value?.Trim() ?? string.Empty);
}
