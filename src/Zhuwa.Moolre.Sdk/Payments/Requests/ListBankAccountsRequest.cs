namespace Zhuwa.Moolre.Payments;

/// <summary>Parameters for the list bank accounts operation.</summary>
public class ListBankAccountsRequest
{
    /// <summary>Account number. Moolre field <c>accountnumber</c>.</summary>
    public string? AccountNumber { get; init; }
}
