namespace Zhuwa.Moolre;

/// <summary>An extensible string value. Moolre can add new values, so this is not an enum: unknown values are kept exactly as received.</summary>
public readonly struct MoolreResponseCode : IEquatable<MoolreResponseCode>
{
    /// <summary>Creates a value from its text.</summary>
    /// <param name="value">The exact text Moolre sends or expects.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null, empty or white space.</exception>
    public MoolreResponseCode(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>The exact text of the value (empty for a default instance).</summary>
    public string Value { get => field ?? string.Empty; }

    /// <summary>Creates a value from its text.</summary>
    /// <param name="value">The exact text Moolre sends or expects.</param>
    /// <returns>The value.</returns>
    public static MoolreResponseCode FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(MoolreResponseCode other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is MoolreResponseCode other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>Whether two values have the same text (case-sensitive).</summary>
    public static bool operator ==(MoolreResponseCode left, MoolreResponseCode right) => left.Equals(right);

    /// <summary>Whether two values have different text.</summary>
    public static bool operator !=(MoolreResponseCode left, MoolreResponseCode right) => !left.Equals(right);

    /// <summary>Converts text to a value.</summary>
    public static implicit operator MoolreResponseCode(string value) => new(value);

    /// <summary>Wallet created (<c>WC02</c>).</summary>
    public static MoolreResponseCode WalletCreated { get; } = new("WC02");

    /// <summary>Wallet created alt (<c>APGW00</c>).</summary>
    public static MoolreResponseCode WalletCreatedAlt { get; } = new("APGW00");

    /// <summary>Account updated (<c>WCU02</c>).</summary>
    public static MoolreResponseCode AccountUpdated { get; } = new("WCU02");

    /// <summary>Wallet updated (<c>APGW16</c>).</summary>
    public static MoolreResponseCode WalletUpdated { get; } = new("APGW16");

    /// <summary>Update invalid account number (<c>WCU01</c>).</summary>
    public static MoolreResponseCode UpdateInvalidAccountNumber { get; } = new("WCU01");

    /// <summary>Currency not supported for API (<c>APGW06</c>).</summary>
    public static MoolreResponseCode CurrencyNotSupportedForApi { get; } = new("APGW06");

    /// <summary>Wallet not found on update (<c>APGW12</c>).</summary>
    public static MoolreResponseCode WalletNotFoundOnUpdate { get; } = new("APGW12");

    /// <summary>Duplicate account name (<c>APGW21</c>).</summary>
    public static MoolreResponseCode DuplicateAccountName { get; } = new("APGW21");

    /// <summary>Wallet found (<c>SW01</c>).</summary>
    public static MoolreResponseCode WalletFound { get; } = new("SW01");

    /// <summary>Transactions found (<c>ST08</c>).</summary>
    public static MoolreResponseCode TransactionsFound { get; } = new("ST08");

    /// <summary>Transactions not found (<c>ST09</c>).</summary>
    public static MoolreResponseCode TransactionsNotFound { get; } = new("ST09");

    /// <summary>Authentication error (<c>AIN01</c>).</summary>
    public static MoolreResponseCode AuthenticationError { get; } = new("AIN01");

    /// <summary>Wallet not found (<c>AIN03</c>).</summary>
    public static MoolreResponseCode WalletNotFound { get; } = new("AIN03");

    /// <summary>API access not activated (<c>AIN04</c>).</summary>
    public static MoolreResponseCode ApiAccessNotActivated { get; } = new("AIN04");

    /// <summary>Validate name authentication error (<c>IN01</c>).</summary>
    public static MoolreResponseCode ValidateNameAuthenticationError { get; } = new("IN01");

    /// <summary>Name validated (<c>AVD01</c>).</summary>
    public static MoolreResponseCode NameValidated { get; } = new("AVD01");

    /// <summary>Name not found (<c>AVD02</c>).</summary>
    public static MoolreResponseCode NameNotFound { get; } = new("AVD02");

    /// <summary>Payout successful (<c>OBGH01</c>).</summary>
    public static MoolreResponseCode PayoutSuccessful { get; } = new("OBGH01");

    /// <summary>Payout failed (<c>APY17</c>).</summary>
    public static MoolreResponseCode PayoutFailed { get; } = new("APY17");

    /// <summary>Payout duplicate reference (<c>APY18</c>).</summary>
    public static MoolreResponseCode PayoutDuplicateReference { get; } = new("APY18");

    /// <summary>Insufficient wallet balance (<c>APY21</c>).</summary>
    public static MoolreResponseCode InsufficientWalletBalance { get; } = new("APY21");

    /// <summary>Payout in progress (<c>OTGH02</c>).</summary>
    public static MoolreResponseCode PayoutInProgress { get; } = new("OTGH02");

    /// <summary>Transfer initiated (<c>TR099</c>).</summary>
    public static MoolreResponseCode TransferInitiated { get; } = new("TR099");

    /// <summary>Internal transfer duplicate reference (<c>TR013</c>).</summary>
    public static MoolreResponseCode InternalTransferDuplicateReference { get; } = new("TR013");

    /// <summary>Internal transfer insufficient balance (<c>TR09</c>).</summary>
    public static MoolreResponseCode InternalTransferInsufficientBalance { get; } = new("TR09");

    /// <summary>Internal transfer completed (<c>OM03</c>).</summary>
    public static MoolreResponseCode InternalTransferCompleted { get; } = new("OM03");

    /// <summary>Transaction successful (<c>SS01</c>).</summary>
    public static MoolreResponseCode TransactionSuccessful { get; } = new("SS01");

    /// <summary>Currency invalid (<c>TP01</c>).</summary>
    public static MoolreResponseCode CurrencyInvalid { get; } = new("TP01");

    /// <summary>Channel not supported (<c>TP09</c>).</summary>
    public static MoolreResponseCode ChannelNotSupported { get; } = new("TP09");

    /// <summary>Collection duplicate reference (<c>TP13</c>).</summary>
    public static MoolreResponseCode CollectionDuplicateReference { get; } = new("TP13");

    /// <summary>OTP required (<c>TP14</c>).</summary>
    public static MoolreResponseCode OtpRequired { get; } = new("TP14");

    /// <summary>Phone verified (<c>TP17</c>).</summary>
    public static MoolreResponseCode PhoneVerified { get; } = new("TP17");

    /// <summary>Payment succeeded (<c>P01</c>).</summary>
    public static MoolreResponseCode PaymentSucceeded { get; } = new("P01");

    /// <summary>Payment failed (<c>P02</c>).</summary>
    public static MoolreResponseCode PaymentFailed { get; } = new("P02");

    /// <summary>Payment ID created (<c>AD14</c>).</summary>
    public static MoolreResponseCode PaymentIdCreated { get; } = new("AD14");

    /// <summary>Bank account created (<c>AD19</c>).</summary>
    public static MoolreResponseCode BankAccountCreated { get; } = new("AD19");

    /// <summary>Bank account creation failed (<c>AD32</c>).</summary>
    public static MoolreResponseCode BankAccountCreationFailed { get; } = new("AD32");

    /// <summary>Payment account found (<c>AC01</c>).</summary>
    public static MoolreResponseCode PaymentAccountFound { get; } = new("AC01");

    /// <summary>Payment account not found (<c>AC03</c>).</summary>
    public static MoolreResponseCode PaymentAccountNotFound { get; } = new("AC03");

    /// <summary>Payment link created (<c>POS09</c>).</summary>
    public static MoolreResponseCode PaymentLinkCreated { get; } = new("POS09");

    /// <summary>Payment link duplicate (<c>INP02</c>).</summary>
    public static MoolreResponseCode PaymentLinkDuplicate { get; } = new("INP02");

    /// <summary>Banks found (<c>SD01</c>).</summary>
    public static MoolreResponseCode BanksFound { get; } = new("SD01");

    /// <summary>Banks found alt (<c>DA01</c>).</summary>
    public static MoolreResponseCode BanksFoundAlt { get; } = new("DA01");

    /// <summary>SMS sent (<c>SMS01</c>).</summary>
    public static MoolreResponseCode SmsSent { get; } = new("SMS01");

    /// <summary>Sender ID not approved (<c>ASMS07</c>).</summary>
    public static MoolreResponseCode SenderIdNotApproved { get; } = new("ASMS07");

    /// <summary>Sender ID request unknown (<c>ASMS09</c>).</summary>
    public static MoolreResponseCode SenderIdRequestUnknown { get; } = new("ASMS09");

    /// <summary>Sender ID status (<c>ASMQ01</c>).</summary>
    public static MoolreResponseCode SenderIdStatus { get; } = new("ASMQ01");

    /// <summary>SMS balance (<c>ASMQ03</c>).</summary>
    public static MoolreResponseCode SmsBalance { get; } = new("ASMQ03");

    /// <summary>Sender ID required (<c>ASMQ06</c>).</summary>
    public static MoolreResponseCode SenderIdRequired { get; } = new("ASMQ06");

    /// <summary>Sender ids updated (<c>ASMQ07</c>).</summary>
    public static MoolreResponseCode SenderIdsUpdated { get; } = new("ASMQ07");

    /// <summary>Sender ID list (<c>ASMQ08</c>).</summary>
    public static MoolreResponseCode SenderIdList { get; } = new("ASMQ08");

    /// <summary>Sender ID no permission (<c>ASMQ09</c>).</summary>
    public static MoolreResponseCode SenderIdNoPermission { get; } = new("ASMQ09");

    /// <summary>SMS status (<c>ASMQ10</c>).</summary>
    public static MoolreResponseCode SmsStatus { get; } = new("ASMQ10");

    /// <summary>Sender ids created (<c>ASMQ12</c>).</summary>
    public static MoolreResponseCode SenderIdsCreated { get; } = new("ASMQ12");

    /// <summary>WhatsApp success (<c>WAS200</c>).</summary>
    public static MoolreResponseCode WhatsAppSuccess { get; } = new("WAS200");

    /// <summary>WhatsApp insufficient balance (<c>WAS401</c>).</summary>
    public static MoolreResponseCode WhatsAppInsufficientBalance { get; } = new("WAS401");

    /// <summary>Bundle catalog (<c>ASB02</c>).</summary>
    public static MoolreResponseCode BundleCatalog { get; } = new("ASB02");

    /// <summary>Sale authentication error (<c>ASL00</c>).</summary>
    public static MoolreResponseCode SaleAuthenticationError { get; } = new("ASL00");

    /// <summary>Sale amount invalid (<c>ASL01</c>).</summary>
    public static MoolreResponseCode SaleAmountInvalid { get; } = new("ASL01");

    /// <summary>Sale receiver missing (<c>ASL02</c>).</summary>
    public static MoolreResponseCode SaleReceiverMissing { get; } = new("ASL02");

    /// <summary>Sale incomplete (<c>ASL03</c>).</summary>
    public static MoolreResponseCode SaleIncomplete { get; } = new("ASL03");

    /// <summary>Sale channel not supported (<c>ASL04</c>).</summary>
    public static MoolreResponseCode SaleChannelNotSupported { get; } = new("ASL04");

    /// <summary>Sale type invalid (<c>ASL05</c>).</summary>
    public static MoolreResponseCode SaleTypeInvalid { get; } = new("ASL05");

    /// <summary>Sale outcome unknown (<c>ASL06</c>).</summary>
    public static MoolreResponseCode SaleOutcomeUnknown { get; } = new("ASL06");

    /// <summary>Sale duplicate reference (<c>ASL08</c>).</summary>
    public static MoolreResponseCode SaleDuplicateReference { get; } = new("ASL08");

    /// <summary>Sale currency not ghs (<c>ASL09</c>).</summary>
    public static MoolreResponseCode SaleCurrencyNotGhs { get; } = new("ASL09");

    /// <summary>Sale wallet currency not supported (<c>ASL10</c>).</summary>
    public static MoolreResponseCode SaleWalletCurrencyNotSupported { get; } = new("ASL10");

    /// <summary>Sale in progress (<c>ASL11</c>).</summary>
    public static MoolreResponseCode SaleInProgress { get; } = new("ASL11");

    /// <summary>Sale wallet inactive (<c>ASL12</c>).</summary>
    public static MoolreResponseCode SaleWalletInactive { get; } = new("ASL12");

    /// <summary>Sale account not found (<c>ASL13</c>).</summary>
    public static MoolreResponseCode SaleAccountNotFound { get; } = new("ASL13");

    /// <summary>Sale bundle missing (<c>ASL14</c>).</summary>
    public static MoolreResponseCode SaleBundleMissing { get; } = new("ASL14");

    /// <summary>Sale bundle not on network (<c>ASL15</c>).</summary>
    public static MoolreResponseCode SaleBundleNotOnNetwork { get; } = new("ASL15");

    /// <summary>Sale status authentication error (<c>ASS00</c>).</summary>
    public static MoolreResponseCode SaleStatusAuthenticationError { get; } = new("ASS00");

    /// <summary>Sale sent (<c>ASS01</c>).</summary>
    public static MoolreResponseCode SaleSent { get; } = new("ASS01");

    /// <summary>Sale failed and refunded (<c>ASS02</c>).</summary>
    public static MoolreResponseCode SaleFailedAndRefunded { get; } = new("ASS02");

    /// <summary>Sale pending (<c>ASS03</c>).</summary>
    public static MoolreResponseCode SalePending { get; } = new("ASS03");

    /// <summary>Sale not found (<c>ASS04</c>).</summary>
    public static MoolreResponseCode SaleNotFound { get; } = new("ASS04");

    /// <summary>Sale ID type invalid (<c>ASS05</c>).</summary>
    public static MoolreResponseCode SaleIdTypeInvalid { get; } = new("ASS05");

    /// <summary>Sale status incomplete (<c>ASS06</c>).</summary>
    public static MoolreResponseCode SaleStatusIncomplete { get; } = new("ASS06");

    /// <summary>Sale status type invalid (<c>ASS07</c>).</summary>
    public static MoolreResponseCode SaleStatusTypeInvalid { get; } = new("ASS07");

    /// <summary>Sale status unknown (<c>ASS08</c>).</summary>
    public static MoolreResponseCode SaleStatusUnknown { get; } = new("ASS08");
}
