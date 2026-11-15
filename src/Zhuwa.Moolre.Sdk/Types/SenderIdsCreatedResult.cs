namespace Zhuwa.Moolre;

/// <summary>Result of the sender ids created operation.</summary>
public class SenderIdsCreatedResult : MoolreResult
{
    internal SenderIdsCreatedResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode)
        : base(code, message, messages, httpStatusCode)
    {
    }


}
