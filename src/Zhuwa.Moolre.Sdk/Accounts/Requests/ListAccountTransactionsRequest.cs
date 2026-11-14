namespace Zhuwa.Moolre.Accounts;

/// <summary>Parameters for the list account transactions operation.</summary>
public class ListAccountTransactionsRequest
{
    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }

    /// <summary>From. Moolre field <c>startdate</c> "YYYY-MM-DD HH:MM:SS".</summary>
    public DateTimeOffset? From { get; init; }

    /// <summary>To. Moolre field <c>enddate</c> (defaults to end of current year).</summary>
    public DateTimeOffset? To { get; init; }

    /// <summary>Limit. Moolre field <c>limit</c> (sent as a string).</summary>
    public int? Limit { get; init; }

    /// <summary>Status. Moolre field <c>status</c> filter 0 / 1 / 2.</summary>
    public TransactionStatus? Status { get; init; }
}
