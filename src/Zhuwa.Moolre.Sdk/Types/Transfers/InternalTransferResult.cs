namespace Zhuwa.Moolre;

/// <summary>Result of the internal transfer operation.</summary>
public class InternalTransferResult : MoolreResult
{
    internal InternalTransferResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        string transferId)
        : base(code, message, messages, httpStatusCode)
    {
        TransferId = transferId;
    }

    /// <summary>Transfer ID. Moolre field <c>data</c> (a UUID string). Confirm the final state with Transfers.GetStatusAsync.</summary>
    public string TransferId { get; }
}
