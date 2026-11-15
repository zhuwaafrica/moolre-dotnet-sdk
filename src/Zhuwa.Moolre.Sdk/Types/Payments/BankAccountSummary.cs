
namespace Zhuwa.Moolre;

/// <summary>Bank account summary.</summary>
public class BankAccountSummary
{
    internal BankAccountSummary(
        string bankAccountNumber,
        string accountName,
        string bankName,
        string externalReference,
        DateTimeOffset createdAt)
    {
        BankAccountNumber = bankAccountNumber;
        AccountName = accountName;
        BankName = bankName;
        ExternalReference = externalReference;
        CreatedAt = createdAt;
    }

    /// <summary>Bank account number. Moolre field <c>accountno</c>.</summary>
    public string BankAccountNumber { get; }

    /// <summary>Account name. Moolre field <c>accountname</c>.</summary>
    public string AccountName { get; }

    /// <summary>Bank name. Moolre field <c>bankname</c> (may be empty).</summary>
    public string BankName { get; }

    /// <summary>External reference. Moolre field <c>extid</c> (may be empty).</summary>
    public string ExternalReference { get; }

    /// <summary>Created at. Moolre field <c>ts</c>.</summary>
    public DateTimeOffset CreatedAt { get; }
}
