namespace Zhuwa.Moolre.Payments;

/// <summary>Result of the mobile money operation.</summary>
public class MobileMoneyResult : MoolreResult
{
    internal MobileMoneyResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        MobileMoneyOutcome outcome,
        string? requestId)
        : base(code, message, messages, httpStatusCode)
    {
        Outcome = outcome;
        RequestId = requestId;
    }

    /// <summary>Outcome.</summary>
    public MobileMoneyOutcome Outcome { get; }

    /// <summary>Request ID. Moolre field <c>data</c> (UUID string) when PromptSent.</summary>
    public string? RequestId { get; }
}
