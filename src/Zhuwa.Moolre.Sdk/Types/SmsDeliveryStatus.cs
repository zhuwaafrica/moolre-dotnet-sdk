namespace Zhuwa.Moolre;

/// <summary>SMS delivery status.</summary>
public class SmsDeliveryStatus
{
    internal SmsDeliveryStatus(
        string reference,
        int status)
    {
        Reference = reference;
        Status = status;
    }

    /// <summary>Reference. Moolre field <c>ref</c>.</summary>
    public string Reference { get; }

    /// <summary>Status. Moolre field <c>status</c> OPEN: meaning of 2, 3, … is not documented.</summary>
    public int Status { get; }
}
