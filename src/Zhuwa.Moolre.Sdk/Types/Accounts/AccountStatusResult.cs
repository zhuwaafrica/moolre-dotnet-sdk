namespace Zhuwa.Moolre;

/// <summary>Result of the account status operation.</summary>
public class AccountStatusResult : MoolreResult
{
    internal AccountStatusResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        decimal balance,
        string accountName,
        bool isActivated,
        Uri? callbackUrl)
        : base(code, message, messages, httpStatusCode)
    {
        Balance = balance;
        AccountName = accountName;
        IsActivated = isActivated;
        CallbackUrl = callbackUrl;
    }

    /// <summary>Balance. Moolre field <c>data.balance</c> (number).</summary>
    public decimal Balance { get; }

    /// <summary>Account name. Moolre field <c>data.accountname</c>.</summary>
    public string AccountName { get; }

    /// <summary>Is activated. Moolre field <c>data.activated</c> ("1").</summary>
    public bool IsActivated { get; }

    /// <summary>Callback URL. Moolre field <c>data.callback</c>.</summary>
    public Uri? CallbackUrl { get; }
}
