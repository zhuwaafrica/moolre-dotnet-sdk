namespace Zhuwa.Moolre.Payments;

/// <summary>Result of the payment account lookup operation.</summary>
public class PaymentAccountLookupResult : MoolreResult
{
    internal PaymentAccountLookupResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        PaymentAccountSummary? account,
        IReadOnlyList<MoolreTransaction> transactions)
        : base(code, message, messages, httpStatusCode)
    {
        Account = account;
        Transactions = transactions;
    }

    /// <summary>Whether the Payment ID or bank account was found (code AC01; AC03 means not found).</summary>
    public bool Found => Code == MoolreResponseCode.PaymentAccountFound;

    /// <summary>Account. Moolre field <c>data.account</c> (null when not found).</summary>
    public PaymentAccountSummary? Account { get; }

    /// <summary>Transactions. Moolre field <c>data.transactions</c> — accepts an array, a single object, or {}.</summary>
    public IReadOnlyList<MoolreTransaction> Transactions { get; }
}
