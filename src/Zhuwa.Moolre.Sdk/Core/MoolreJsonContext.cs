using System.Text.Json;
using System.Text.Json.Serialization;
using Zhuwa.Moolre.Accounts;
using Zhuwa.Moolre.Payments;
using Zhuwa.Moolre.Reference;
using Zhuwa.Moolre.Sales;
using Zhuwa.Moolre.Sms;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// The source-generated JSON contract for every type the SDK sends or reads. The field names live on the types themselves
/// (<c>[JsonPropertyName]</c>); this context turns them into serialization code at build time, so no reflection runs (Microsoft's
/// documented approach for trimming and Native AOT). A new request or response type must be listed here.
/// </summary>
[JsonSourceGenerationOptions(
    // Unset optional values are left out of the request, so Moolre only sees what the caller set.
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    // Moolre's loose value shapes, applied to every property of these types unless a property names its own converter.
    Converters =
    [
        typeof(MoolreStringConverter),
        typeof(LenientDecimalConverter),
        typeof(LenientInt32Converter),
        typeof(LenientBooleanConverter),
        typeof(MoolreTimestampConverter),
        typeof(AbsoluteUriConverter),
        typeof(CurrencyConverter),
        typeof(TransferChannelConverter),
        typeof(PaymentChannelConverter),
        typeof(TransactionStatusConverter),
        typeof(TransactionIdKindConverter),
        typeof(SalesNetworkConverter),
        typeof(SaleIdKindConverter),
        typeof(LenientDoubleConverter),
    ])]

// Requests
[JsonSerializable(typeof(GetAccountStatusRequest))]
[JsonSerializable(typeof(CreateSandboxAccountRequest))]
[JsonSerializable(typeof(UpdateAccountRequest))]
[JsonSerializable(typeof(ListAccountTransactionsRequest))]
[JsonSerializable(typeof(ValidateNameRequest))]
[JsonSerializable(typeof(SendTransferRequest))]
[JsonSerializable(typeof(GetTransferStatusRequest))]
[JsonSerializable(typeof(SendInternalTransferRequest))]
[JsonSerializable(typeof(RequestMobileMoneyRequest))]
[JsonSerializable(typeof(GetPaymentStatusRequest))]
[JsonSerializable(typeof(CreatePaymentLinkRequest))]
[JsonSerializable(typeof(CreatePaymentIdRequest))]
[JsonSerializable(typeof(CreateBankAccountRequest))]
[JsonSerializable(typeof(ListBankAccountsRequest))]
[JsonSerializable(typeof(GetPaymentAccountRequest))]
[JsonSerializable(typeof(GetBanksRequest))]
[JsonSerializable(typeof(SendAirtimeRequest))]
[JsonSerializable(typeof(SendDataRequest))]
[JsonSerializable(typeof(ListBundlesRequest))]
[JsonSerializable(typeof(GetSaleStatusRequest))]
[JsonSerializable(typeof(SendSmsRequest))]
[JsonSerializable(typeof(SendSingleSmsRequest))]
[JsonSerializable(typeof(GetSmsStatusRequest))]
[JsonSerializable(typeof(CreateSenderIdsRequest))]
[JsonSerializable(typeof(GetSenderIdStatusRequest))]
[JsonSerializable(typeof(ApproveSenderIdsRequest))]

// Responses
[JsonSerializable(typeof(ResponseEnvelopeWire))]
[JsonSerializable(typeof(TransactionWire))]
[JsonSerializable(typeof(List<TransactionWire>))]
[JsonSerializable(typeof(AccountStatusWire))]
[JsonSerializable(typeof(AccountWire))]
[JsonSerializable(typeof(SettlementSettings))]
[JsonSerializable(typeof(AccountTransactionsWire))]
[JsonSerializable(typeof(BankWire))]
[JsonSerializable(typeof(TransferWire))]
[JsonSerializable(typeof(PaymentLinkWire))]
[JsonSerializable(typeof(PaymentIdWire))]
[JsonSerializable(typeof(BankAccountWire))]
[JsonSerializable(typeof(BankAccountListWire))]
[JsonSerializable(typeof(BankAccountItemWire))]
[JsonSerializable(typeof(List<BankAccountItemWire>))]
[JsonSerializable(typeof(PaymentAccountLookupWire))]
[JsonSerializable(typeof(PaymentAccountWire))]
[JsonSerializable(typeof(PaymentWebhookWire))]
[JsonSerializable(typeof(SaleWire))]
[JsonSerializable(typeof(BundleCatalogWire))]
[JsonSerializable(typeof(BundleWire))]
[JsonSerializable(typeof(List<BundleWire>))]
[JsonSerializable(typeof(SaleStatusWire))]
[JsonSerializable(typeof(SmsStatusWire))]
[JsonSerializable(typeof(SenderIdWire))]
[JsonSerializable(typeof(SmsBalanceWire))]
[JsonSerializable(typeof(JsonElement))]
internal sealed partial class MoolreJsonContext : JsonSerializerContext;
