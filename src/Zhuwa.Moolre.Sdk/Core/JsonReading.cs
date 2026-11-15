using System.Globalization;
using System.Text.Json;

namespace Zhuwa.Moolre.Core;

/// <summary>Small helpers the tolerant converters share, so each reads Moolre's loose values the same way.</summary>
internal static class JsonReading
{
    /// <summary>Skips the current value (whole object or array included) and returns the default, for a value of an unexpected shape.</summary>
    internal static T? SkipValue<T>(ref Utf8JsonReader reader)
    {
        reader.Skip();
        return default;
    }

    /// <summary>A decimal sent as a number or as text, or <see langword="null"/> when it is neither.</summary>
    internal static decimal? ReadDecimal(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.TryGetDecimal(out var number) ? number : null;
            case JsonTokenType.String:
                return decimal.TryParse(reader.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
            default:
                reader.Skip();
                return null;
        }
    }

    /// <summary>An integer sent as a number or as text, or <see langword="null"/> when it is neither.</summary>
    internal static int? ReadInt32(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.TryGetInt32(out var number) ? number : null;
            case JsonTokenType.String:
                return int.TryParse(reader.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
            default:
                reader.Skip();
                return null;
        }
    }
}
