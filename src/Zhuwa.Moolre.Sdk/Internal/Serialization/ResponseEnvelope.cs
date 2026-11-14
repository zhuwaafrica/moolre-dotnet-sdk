using System.Net;
using System.Text.Json;

namespace Zhuwa.Moolre.Internal.Serialization;

/// <summary>
/// The parts every Moolre response shares: <c>status</c>, <c>code</c>, <c>message</c> and <c>data</c>.
/// The shapes vary (status is a number or a string, message is a string, an array or <c>null</c>, data is anything),
/// so this reads them by hand instead of binding to a fixed type.
/// </summary>
internal sealed class ResponseEnvelope
{
    private ResponseEnvelope(
        bool isSuccess,
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        JsonElement data,
        HttpStatusCode httpStatusCode)
    {
        IsSuccess = isSuccess;
        Code = code;
        Message = message;
        Messages = messages;
        Data = data;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>Whether <c>status</c> is <c>1</c> (as a number or as the string <c>"1"</c>).</summary>
    internal bool IsSuccess { get; }

    /// <summary>Moolre's application code; the default value when the response had none.</summary>
    internal MoolreResponseCode Code { get; }

    /// <summary>The message (an array is joined with a space), or <see langword="null"/>.</summary>
    internal string? Message { get; }

    /// <summary>The messages, one per string; empty when none.</summary>
    internal IReadOnlyList<string> Messages { get; }

    /// <summary>The <c>data</c> value; <see cref="JsonValueKind.Undefined"/> when absent. Safe to use after parsing.</summary>
    internal JsonElement Data { get; }

    internal HttpStatusCode HttpStatusCode { get; }

    /// <summary><c>data</c> when it is a string (for example <c>"senderid"</c>), otherwise <see langword="null"/>.</summary>
    internal string? Detail => Data.ValueKind == JsonValueKind.String ? Data.GetString() : null;

    /// <summary>Reads a response body.</summary>
    /// <exception cref="MoolreResponseException">The body is not a JSON object with a <c>status</c> or a <c>code</c>.</exception>
    internal static ResponseEnvelope Parse(ReadOnlySpan<byte> body, HttpStatusCode httpStatusCode)
    {
        try
        {
            // ParseValue reads one value from the span and ignores anything after it. The document is disposed straight away;
            // Data is cloned so it stays usable.
            var reader = new Utf8JsonReader(body);
            using var document = JsonDocument.ParseValue(ref reader);
            return Read(document.RootElement, httpStatusCode);
        }
        catch (JsonException ex)
        {
            throw new MoolreResponseException("Moolre returned a body that is not valid JSON.", httpStatusCode, ex);
        }
    }

    private static ResponseEnvelope Read(JsonElement root, HttpStatusCode httpStatusCode)
    {
        // A Moolre answer is always an object with at least a status or a code. Anything else (an array, a string, a proxy's
        // JSON error page) is not ours and must not be treated as a result. The single "|" evaluates both sides, which
        // assigns the status variable even when the code check is not needed.
        if (root.ValueKind != JsonValueKind.Object
            || !(root.TryGetProperty("status", out var status) | root.TryGetProperty("code", out _)))
        {
            throw new MoolreResponseException("Moolre returned JSON that is not a Moolre response.", httpStatusCode);
        }

        // Status arrives as 1 or "1" (success) and 0 or "0" (failure), sometimes for the same endpoint family.
        // Anything else counts as not successful.
        var isSuccess = status.ValueKind switch
        {
            JsonValueKind.Number => status.TryGetInt32(out var number) && number == 1,
            JsonValueKind.String => string.Equals(status.GetString(), "1", StringComparison.Ordinal),
            _ => false,
        };

        // The code is the stable value to branch on. A missing code becomes the default (empty) value, not an error.
        var codeText = root.ReadString("code");
        var code = string.IsNullOrWhiteSpace(codeText) ? default : new MoolreResponseCode(codeText);

        var messages = ReadMessages(root);
        // Clone detaches the value from the JsonDocument, which is disposed when Parse returns.
        var data = root.TryGetProperty("data", out var dataValue) ? dataValue.Clone() : default;

        return new ResponseEnvelope(
            isSuccess,
            code,
            messages.Count == 0 ? null : string.Join(' ', messages),
            messages,
            data,
            httpStatusCode);
    }

    private static List<string> ReadMessages(JsonElement root)
    {
        // Moolre sends message as a string, an array of strings, null, or leaves it out. All four become a list.
        var messages = new List<string>();
        if (!root.TryGetProperty("message", out var message))
        {
            return messages;
        }

        if (message.ValueKind == JsonValueKind.String)
        {
            if (message.GetString() is { Length: > 0 } text)
            {
                messages.Add(text);
            }
        }
        else if (message.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in message.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String && item.GetString() is { Length: > 0 } text)
                {
                    messages.Add(text);
                }
            }
        }

        return messages;
    }
}
