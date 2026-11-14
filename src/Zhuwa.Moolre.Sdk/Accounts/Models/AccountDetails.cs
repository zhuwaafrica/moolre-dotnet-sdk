namespace Zhuwa.Moolre.Accounts;

/// <summary>Account details.</summary>
public class AccountDetails
{
    internal AccountDetails(
        bool isEnabled,
        string accountNumber,
        string accountName,
        string paymentId,
        bool apiEnabled,
        Uri? callbackUrl,
        SettlementSettings settlement,
        string secret)
    {
        IsEnabled = isEnabled;
        AccountNumber = accountNumber;
        AccountName = accountName;
        PaymentId = paymentId;
        ApiEnabled = apiEnabled;
        CallbackUrl = callbackUrl;
        Settlement = settlement;
        Secret = secret;
    }

    /// <summary>Is enabled. Moolre field <c>data.status</c> (1).</summary>
    public bool IsEnabled { get; }

    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string AccountNumber { get; }

    /// <summary>Account name. Moolre field <c>accountname</c>.</summary>
    public string AccountName { get; }

    /// <summary>Payment ID. Moolre field <c>paymentid</c>.</summary>
    public string PaymentId { get; }

    /// <summary>API enabled. Moolre field <c>api</c> (0 / 1).</summary>
    public bool ApiEnabled { get; }

    /// <summary>Callback URL. Moolre field <c>callback</c> ("" becomes null).</summary>
    public Uri? CallbackUrl { get; }

    /// <summary>Settlement. Moolre field <c>settlement</c> ({} becomes empty settings).</summary>
    public SettlementSettings Settlement { get; }

    /// <summary>Secret. Moolre field <c>secret</c> — the callback verification secret; sensitive, never logged.</summary>
    public string Secret { get; }
}
