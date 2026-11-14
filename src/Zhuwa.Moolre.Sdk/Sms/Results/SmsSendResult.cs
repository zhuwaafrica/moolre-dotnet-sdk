namespace Zhuwa.Moolre.Sms;

/// <summary>Result of the SMS send operation.</summary>
public class SmsSendResult : MoolreResult
{
    internal SmsSendResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode)
        : base(code, message, messages, httpStatusCode)
    {
    }


}
