namespace Zhuwa.Moolre;

/// <summary>Result of the SMS status operation.</summary>
public class SmsStatusResult : MoolreResult
{
    internal SmsStatusResult(
        MoolreResponseCode code,
        string? message,
        IReadOnlyList<string> messages,
        System.Net.HttpStatusCode httpStatusCode,
        IReadOnlyList<SmsDeliveryStatus> statuses)
        : base(code, message, messages, httpStatusCode)
    {
        Statuses = statuses;
    }

    /// <summary>Statuses. Moolre field <c>data</c>.</summary>
    public IReadOnlyList<SmsDeliveryStatus> Statuses { get; }
}
