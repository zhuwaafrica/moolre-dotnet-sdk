namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the create payment link operation.</summary>
public class CreatePaymentLinkRequest
{
    /// <summary>Amount. Moolre field <c>amount</c> (string).</summary>
    public required decimal Amount { get; init; }

    /// <summary>Email. Moolre field <c>email</c> (the business's email).</summary>
    public required string Email { get; init; }

    /// <summary>External reference. Moolre field <c>externalref</c> (required, unique).</summary>
    public required string ExternalReference { get; init; }

    /// <summary>Callback URL. Moolre field <c>callback</c>.</summary>
    public Uri? CallbackUrl { get; init; }

    /// <summary>Redirect URL. Moolre field <c>redirect</c>.</summary>
    public Uri? RedirectUrl { get; init; }

    /// <summary>Is reusable. Moolre field <c>reusable</c> ("0" / "1").</summary>
    public bool IsReusable { get; init; }

    /// <summary>Expires after. Moolre field <c>expiration_time</c> (whole minutes, minimum 1).</summary>
    public TimeSpan? ExpiresAfter { get; init; }

    /// <summary>Currency. Moolre field <c>currency</c>.</summary>
    public required Currency Currency { get; init; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>
    /// Extra values Moolre stores with the payment and returns in the callback, for example an order ID. Moolre field
    /// <c>metadata</c>. Moolre documents it only as an object, so the SDK sends text values.
    /// </summary>
    public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>();
}
