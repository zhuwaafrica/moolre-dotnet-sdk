namespace Zhuwa.Moolre;

/// <summary>The transaction details inside a payment webhook.</summary>
public class PaymentWebhookData
{
    internal PaymentWebhookData(
        TransactionStatus status,
        string payer,
        string? payerName,
        string? payerNetwork,
        string? payerInstitution,
        string terminalId,
        string accountNumber,
        string name,
        decimal amount,
        decimal value,
        string? transactionId,
        string externalReference,
        string thirdPartyReference,
        string? reason,
        string secret,
        DateTimeOffset timestamp)
    {
        Status = status;
        Payer = payer;
        PayerName = payerName;
        PayerNetwork = payerNetwork;
        PayerInstitution = payerInstitution;
        TerminalId = terminalId;
        AccountNumber = accountNumber;
        Name = name;
        Amount = amount;
        Value = value;
        TransactionId = transactionId;
        ExternalReference = externalReference;
        ThirdPartyReference = thirdPartyReference;
        Reason = reason;
        // The SDK still fills the deprecated field until Moolre stops sending it; only callers reading it should see the warning.
#pragma warning disable CS0618
        Secret = secret;
#pragma warning restore CS0618
        Timestamp = timestamp;
    }

    /// <summary>Status. Moolre field <c>txstatus</c>.</summary>
    public TransactionStatus Status { get; }

    /// <summary>Payer. Moolre field <c>payer</c>.</summary>
    public string Payer { get; }

    /// <summary>Payer name. Moolre field <c>payername</c> (added 11 Nov 2026; may be empty or absent).</summary>
    public string? PayerName { get; }

    /// <summary>Payer network. Moolre field <c>payernetwork</c>.</summary>
    public string? PayerNetwork { get; }

    /// <summary>Payer institution. Moolre field <c>payerinstitution</c>.</summary>
    public string? PayerInstitution { get; }

    /// <summary>Terminal ID. Moolre field <c>terminalid</c>.</summary>
    public string TerminalId { get; }

    /// <summary>Account number. Moolre field <c>accountnumber</c> (destination wallet).</summary>
    public string AccountNumber { get; }

    /// <summary>Name. Moolre field <c>name</c>.</summary>
    public string Name { get; }

    /// <summary>Amount. Moolre field <c>amount</c> ("0" on failure).</summary>
    public decimal Amount { get; }

    /// <summary>Value. Moolre field <c>value</c> ("0" on failure).</summary>
    public decimal Value { get; }

    /// <summary>Transaction ID. Moolre field <c>transactionid</c> (null on failed attempts).</summary>
    public string? TransactionId { get; }

    /// <summary>External reference. Moolre field <c>externalref</c>.</summary>
    public string ExternalReference { get; }

    /// <summary>Third party reference. Moolre field <c>thirdpartyref</c>.</summary>
    public string ThirdPartyReference { get; }

    /// <summary>Reason. Moolre field <c>reason</c> (failed callbacks).</summary>
    public string? Reason { get; }

    /// <summary>
    /// The callback secret Moolre sent. Moolre field <c>secret</c>. Sensitive: never log it, and do not compare it yourself. Empty when the
    /// callback had none. Deprecated by Moolre: verify the signature header with
    /// <see cref="Webhooks.MoolreWebhookVerifier.IsSignatureValid(ReadOnlySpan{byte}, string)"/> instead.
    /// </summary>
    [Obsolete(Webhooks.MoolreWebhookVerifier.SecretFieldObsoleteMessage)]
    public string Secret { get; }

    /// <summary>Timestamp. Moolre field <c>ts</c> (GMT).</summary>
    public DateTimeOffset Timestamp { get; }
}
