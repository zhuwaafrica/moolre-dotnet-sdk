namespace Zhuwa.Moolre.Accounts;

/// <summary>Result of the account operation.</summary>
public class AccountResult : MoolreResult
{
    internal AccountResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        AccountDetails account)
        : base(code, message, messages, httpStatusCode)
    {
        Account = account;
    }

    /// <summary>Account. Moolre field <c>data</c>.</summary>
    public AccountDetails Account { get; }
}
