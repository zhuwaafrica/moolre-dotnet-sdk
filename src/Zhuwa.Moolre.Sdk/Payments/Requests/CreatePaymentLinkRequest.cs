using System.Text.Json.Serialization;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the create payment link operation.</summary>
public class CreatePaymentLinkRequest
{
    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    [JsonPropertyName("amount")]
    [JsonConverter(typeof(DecimalAsTextConverter))]
    public required decimal Amount { get; init; }

    /// <summary>Email. Moolre field <c>email</c> (the business's email).</summary>
    [JsonPropertyName("email")]
    public required string Email { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    [JsonPropertyName("externalref")]
    public required string ExternalReference { get; init; }

    /// <summary>Callback URL. Moolre field <c>callback</c>.</summary>
    [JsonPropertyName("callback")]
    public Uri? CallbackUrl { get; init; }

    /// <summary>Redirect URL. Moolre field <c>redirect</c>.</summary>
    [JsonPropertyName("redirect")]
    public Uri? RedirectUrl { get; init; }

    /// <summary>Is reusable. Moolre field <c>reusable</c> ("0" / "1").</summary>
    [JsonPropertyName("reusable")]
    [JsonConverter(typeof(BooleanAsTextConverter))]
    public bool IsReusable { get; init; }

    /// <summary>Expires after. Moolre field <c>expiration_time</c> (whole minutes, minimum 1).</summary>
    [JsonPropertyName("expiration_time")]
    [JsonConverter(typeof(WholeMinutesConverter))]
    public TimeSpan? ExpiresAfter { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    [JsonPropertyName("currency")]
    public required Currency Currency { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    [JsonPropertyName(MoolreJson.AccountNumberField)]
    public string? AccountNumber { get; init; }

    /// <summary>
    /// Extra values Moolre stores with the payment and returns in the callback, for example an order ID. Moolre field
    /// <c>metadata</c>. Moolre documents it only as an object, so the SDK sends text values.
    /// </summary>
    [JsonPropertyName("metadata")]
    public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
}
