using System.Net;
using System.Text.Json;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// The parts every Moolre response shares: <c>status</c>, <c>code</c>, <c>message</c> and <c>data</c>, read through
/// <see cref="ResponseEnvelopeWire"/>. The shapes vary (status is a number or text, message is text, an array or <c>null</c>, data is
/// anything), so <c>data</c> is kept as a JSON element and read by each operation's mapper.
/// </summary>
internal sealed class ResponseEnvelope
{
    private ResponseEnvelope(
        bool isSuccess,
        MoolreResponseCode code,
        IReadOnlyList<string> messages,
        JsonElement data,
        string? next,
        JsonElement error,
        HttpStatusCode httpStatusCode)
    {
        Next = string.IsNullOrWhiteSpace(next) ? null : next;
        Error = error;
        IsSuccess = isSuccess;
        Code = code;
        Message = messages.Count == 0 ? null : string.Join(' ', messages);
        Messages = messages;
        Data = data;
        HttpStatusCode = httpStatusCode;
    }

    /// <summary>Whether <c>status</c> is <c>1</c> (as a number or as the text <c>"1"</c>).</summary>
    internal bool IsSuccess { get; }

    /// <summary>Moolre's application code; the default value when the response had none.</summary>
    internal MoolreResponseCode Code { get; }

    /// <summary>The message (an array is joined with a space), or <see langword="null"/>.</summary>
    internal string? Message { get; }

    /// <summary>The messages, one per string; empty when none.</summary>
    internal IReadOnlyList<string> Messages { get; }

    /// <summary>The <c>data</c> value; <see cref="JsonValueKind.Undefined"/> when absent. It does not depend on the parsed body.</summary>
    internal JsonElement Data { get; }

    internal HttpStatusCode HttpStatusCode { get; }

    /// <summary>WhatsApp only: the cursor for the next page of templates, or <see langword="null"/> on the last page.</summary>
    internal string? Next { get; }

    /// <summary>WhatsApp only: a send warning as text, or an empty array (or nothing) when there is none.</summary>
    internal JsonElement Error { get; }

    /// <summary><c>data</c> when it is text (for example <c>"senderid"</c>), otherwise <see langword="null"/>.</summary>
    internal string? Detail => MoolreJson.ReadDataText(Data);

    /// <summary>Reads a response body.</summary>
    /// <exception cref="MoolreResponseException">The body is not a JSON object with a <c>status</c> or a <c>code</c>.</exception>
    internal static ResponseEnvelope Parse(ReadOnlySpan<byte> body, HttpStatusCode httpStatusCode)
    {
        ResponseEnvelopeWire? wire;
        try
        {
            wire = JsonSerializer.Deserialize(body, MoolreJsonContext.Default.ResponseEnvelopeWire);
        }
        catch (JsonException ex)
        {
            // Covers bodies that are not JSON at all (an HTML error page) and JSON that is not an object (an array, a string).
            throw new MoolreResponseException("Moolre returned a body that is not a Moolre response.", httpStatusCode, ex);
        }

        // A Moolre answer always has a status or a code; "null", {} or a proxy's own JSON is not ours and must not become a result.
        if (wire is null || (wire.Status.ValueKind == JsonValueKind.Undefined && string.IsNullOrWhiteSpace(wire.Code)))
        {
            throw new MoolreResponseException("Moolre returned JSON that is not a Moolre response.", httpStatusCode);
        }

        // Status arrives as 1 or "1" (success) and 0 or "0" (failure), sometimes for the same endpoint family.
        var isSuccess = wire.Status.ValueKind switch
        {
            JsonValueKind.Number => wire.Status.TryGetInt32(out var number) && number == 1,
            JsonValueKind.String => string.Equals(wire.Status.GetString(), "1", StringComparison.Ordinal),
            _ => false,
        };

        // The code is the stable value to branch on. A missing code becomes the default (empty) value, not an error.
        var code = string.IsNullOrWhiteSpace(wire.Code) ? default : new MoolreResponseCode(wire.Code);

        return new ResponseEnvelope(isSuccess, code, wire.Messages, wire.Data, wire.Next, wire.Error, httpStatusCode);
    }
}
