namespace Zhuwa.Moolre.Payments;

/// <summary>Result of the payment link operation.</summary>
public class PaymentLinkResult : MoolreResult
{
    internal PaymentLinkResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        Uri authorizationUrl,
        string reference)
        : base(code, message, messages, httpStatusCode)
    {
        AuthorizationUrl = authorizationUrl;
        Reference = reference;
    }

    /// <summary>Authorization URL. Moolre field <c>data.authorization_url</c>.</summary>
    public Uri AuthorizationUrl { get; }

    /// <summary>Reference. Moolre field <c>data.reference</c>.</summary>
    public string Reference { get; }
}
