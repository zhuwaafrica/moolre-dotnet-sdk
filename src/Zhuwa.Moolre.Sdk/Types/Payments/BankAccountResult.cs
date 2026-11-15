namespace Zhuwa.Moolre;

/// <summary>Result of the bank account operation.</summary>
public class BankAccountResult : MoolreResult
{
    internal BankAccountResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        string bankAccountNumber,
        string accountName,
        string bankName,
        string externalReference)
        : base(code, message, messages, httpStatusCode)
    {
        BankAccountNumber = bankAccountNumber;
        AccountName = accountName;
        BankName = bankName;
        ExternalReference = externalReference;
    }

    /// <summary>Bank account number. Moolre field <c>data.accountno</c>.</summary>
    public string BankAccountNumber { get; }

    /// <summary>Account name. Moolre field <c>data.accountname</c>.</summary>
    public string AccountName { get; }

    /// <summary>Bank name. Moolre field <c>data.bankname</c>.</summary>
    public string BankName { get; }

    /// <summary>External reference. Moolre field <c>data.externalref</c>.</summary>
    public string ExternalReference { get; }
}
