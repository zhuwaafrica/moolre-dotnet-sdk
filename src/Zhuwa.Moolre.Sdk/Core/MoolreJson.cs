using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Turns requests into JSON bodies and response data into the internal response models, through <see cref="MoolreJsonContext"/>.
/// No field name is written here except the wallet field, which the SDK fills in for every operation.
/// </summary>
internal static class MoolreJson
{
    /// <summary>Moolre's wallet field. Requests declare it with this constant, and <see cref="SetAccountNumber"/> fills in the default.</summary>
    internal const string AccountNumberField = "accountnumber";

    /// <summary>
    /// Serializes a request to a JSON object. Optional text left blank and empty objects (for example metadata with no entries) are
    /// removed, so Moolre only sees values the caller actually set.
    /// </summary>
    internal static JsonObject ToBody<T>(T request, JsonTypeInfo<T> typeInfo)
    {
        var body = JsonSerializer.SerializeToNode(request, typeInfo)?.AsObject() ?? [];

        // Required text was checked before serializing, so only optional values can be blank here.
        var blanks = body
            .Where(pair => pair.Value is JsonValue value && value.GetValueKind() == JsonValueKind.String && value.GetValue<string>().Length == 0
                || pair.Value is JsonObject { Count: 0 })
            .Select(pair => pair.Key)
            .ToList();
        foreach (var key in blanks)
        {
            body.Remove(key);
        }

        return body;
    }

    /// <summary>Sets the wallet field to the resolved account number (the request's, or the default from the options).</summary>
    internal static JsonObject SetAccountNumber(this JsonObject body, string accountNumber)
    {
        body[AccountNumberField] = accountNumber;
        return body;
    }

    /// <summary>Reads Moolre's <c>data</c> object into a response model. Data that is not an object reads as an empty model.</summary>
    /// <exception cref="MoolreResponseException">The data is an object but cannot be read.</exception>
    internal static T ReadData<T>(JsonElement data, JsonTypeInfo<T> typeInfo, HttpStatusCode httpStatusCode)
        where T : new()
    {
        if (data.ValueKind != JsonValueKind.Object)
        {
            return new T();
        }

        try
        {
            return data.Deserialize(typeInfo) ?? new T();
        }
        catch (JsonException ex)
        {
            throw new MoolreResponseException("Moolre returned data the SDK could not read.", httpStatusCode, ex);
        }
    }

    /// <summary>Enumerates objects in a <c>data</c> array. Entries that are not objects are skipped.</summary>
    internal static IEnumerable<T> EnumerateData<T>(JsonElement data, JsonTypeInfo<T> typeInfo)
    {
        if (data.ValueKind != JsonValueKind.Array)
        {
            yield break;
        }

        foreach (var entry in data.EnumerateArray())
        {
            if (entry.ValueKind == JsonValueKind.Object && entry.Deserialize(typeInfo) is { } item)
            {
                yield return item;
            }
        }
    }

    /// <summary><c>data</c> when it is plain text (a name, a request ID, <c>"senderid"</c>), otherwise <see langword="null"/>.</summary>
    internal static string? ReadDataText(JsonElement data) => data.ValueKind == JsonValueKind.String ? data.GetString() : null;

    /// <summary>
    /// The query string for a GET call, built from a request object the same way as a body (<c>country=gha&amp;data=banks</c>).
    /// Every value is escaped, so a value like <c>g&amp;a</c> cannot add a parameter.
    /// </summary>
    internal static string ToQuery(JsonObject query) => string.Join('&', query.Select(pair =>
        $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value is JsonValue value && value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : pair.Value?.ToJsonString() ?? string.Empty)}"));
}
