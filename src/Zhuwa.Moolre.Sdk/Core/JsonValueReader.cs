using System.Globalization;
using System.Text.Json;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Reads values from Moolre's loose JSON. The same field can arrive as a number or a string, and can be absent,
/// so every read accepts both and returns a default instead of throwing.
/// </summary>
internal static class JsonValueReader
{
    /// <summary>
    /// Like <see cref="JsonElement.TryGetProperty(string, out JsonElement)"/>, but returns <see langword="false"/> for a value that is
    /// not an object (a string, <c>null</c> or absent <c>data</c>) instead of throwing.
    /// </summary>
    internal static bool TryGetField(this JsonElement element, string name, out JsonElement value)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out value))
        {
            return true;
        }

        value = default;
        return false;
    }

    /// <summary>A string field, or <see langword="null"/> when absent or <c>null</c>. A number is returned as text.</summary>
    internal static string? ReadString(this JsonElement element, string name)
    {
        if (!element.TryGetField(name, out var value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null,
        };
    }

    /// <summary>A decimal field sent as a number or a string, or <c>0</c> when absent or not a number.</summary>
    internal static decimal ReadDecimal(this JsonElement element, string name)
    {
        if (!element.TryGetField(name, out var value))
        {
            return 0m;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetDecimal(out var number) ? number : 0m,
            JsonValueKind.String => decimal.TryParse(value.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m,
            _ => 0m,
        };
    }

    /// <summary>A flag sent as <c>1</c>, <c>"1"</c> or <c>true</c>; anything else is <see langword="false"/>.</summary>
    internal static bool ReadFlag(this JsonElement element, string name)
    {
        if (!element.TryGetField(name, out var value))
        {
            return false;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.Number => value.TryGetInt32(out var number) && number == 1,
            JsonValueKind.String => string.Equals(value.GetString(), "1", StringComparison.Ordinal)
                || string.Equals(value.GetString(), "true", StringComparison.OrdinalIgnoreCase),
            _ => false,
        };
    }

    /// <summary>An integer field sent as a number or a string, or <paramref name="fallback"/> when absent or not an integer.</summary>
    internal static int ReadInt32(this JsonElement element, string name, int fallback = 0)
    {
        if (!element.TryGetField(name, out var value))
        {
            return fallback;
        }

        return value.ValueKind switch
        {
            JsonValueKind.Number => value.TryGetInt32(out var number) ? number : fallback,
            JsonValueKind.String => int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : fallback,
            _ => fallback,
        };
    }

    /// <summary>
    /// A <c>txstatus</c> field: <c>1</c> successful, <c>2</c> failed, <c>0</c> pending. A missing or unknown value reads as
    /// <see cref="TransactionStatus.Pending"/>, never as successful: the SDK must not report money as moved unless Moolre says so.
    /// </summary>
    internal static TransactionStatus ReadTransactionStatus(this JsonElement element, string name) =>
        element.ReadInt32(name, fallback: -1) switch
        {
            1 => TransactionStatus.Successful,
            2 => TransactionStatus.Failed,
            _ => TransactionStatus.Pending,
        };

    /// <summary>
    /// A Moolre timestamp, <c>yyyy-MM-dd HH:mm:ss</c> in GMT, as a UTC <see cref="DateTimeOffset"/>. Absent or unreadable gives
    /// <see langword="default"/>, so one odd field does not fail the whole result.
    /// </summary>
    internal static DateTimeOffset ReadTimestamp(this JsonElement element, string name) =>
        DateTimeOffset.TryParseExact(
            element.ReadString(name),
            "yyyy-MM-dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var timestamp)
            ? timestamp
            : default;

    /// <summary>A decimal field, or <see langword="null"/> when the field is absent (for fields only some responses carry).</summary>
    internal static decimal? ReadOptionalDecimal(this JsonElement element, string name) =>
        element.TryGetField(name, out _) ? element.ReadDecimal(name) : null;

    /// <summary>An absolute URL field, or <see langword="null"/> when absent, empty or not an absolute URL.</summary>
    internal static Uri? ReadUri(this JsonElement element, string name) =>
        Uri.TryCreate(element.ReadString(name), UriKind.Absolute, out var uri) ? uri : null;
}
