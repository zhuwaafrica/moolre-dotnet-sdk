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

    /// <summary>Moolre's delivery status number. Moolre field <c>status</c>. Moolre does not document what each number means, so it is passed on unchanged.</summary>
    public int Status { get; }
}
