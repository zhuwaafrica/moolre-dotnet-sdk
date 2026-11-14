namespace Zhuwa.Moolre.Sms;

/// <summary>Result of the SMS balance operation.</summary>
public class SmsBalanceResult : MoolreResult
{
    internal SmsBalanceResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        decimal balance)
        : base(code, message, messages, httpStatusCode)
    {
        Balance = balance;
    }

    /// <summary>Balance. Moolre field <c>data.balance</c> (integer in the example).</summary>
    public decimal Balance { get; }
}
