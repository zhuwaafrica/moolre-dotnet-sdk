namespace Zhuwa.Moolre.Payments;

/// <summary>Payment account summary.</summary>
public class PaymentAccountSummary
{
    internal PaymentAccountSummary(
        string bankAccountNumber,
        int transactionCount,
        decimal transactionValue)
    {
        BankAccountNumber = bankAccountNumber;
        TransactionCount = transactionCount;
        TransactionValue = transactionValue;
    }

    /// <summary>Bank account number. Moolre field <c>account.accountno</c>.</summary>
    public string BankAccountNumber { get; }

    /// <summary>Transaction count. Moolre field <c>account.txcount</c> (string).</summary>
    public int TransactionCount { get; }

    /// <summary>Transaction value. Moolre field <c>account.txvalue</c> (string).</summary>
    public decimal TransactionValue { get; }
}
