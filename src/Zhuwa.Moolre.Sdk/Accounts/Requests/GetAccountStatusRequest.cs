namespace Zhuwa.Moolre.Accounts;

/// <summary>Parameters for the get account status operation.</summary>
public class GetAccountStatusRequest
{
    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
