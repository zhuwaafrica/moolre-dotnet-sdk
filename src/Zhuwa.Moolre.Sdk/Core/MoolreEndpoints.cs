

using System.Runtime.CompilerServices;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// The Moolre operations the SDK can call. Each entry holds the HTTP method, path, <c>type</c> value,
/// credential kind and retry safety for one operation. Clients look operations up here and do not hard-code these values.
/// </summary>
internal static class MoolreEndpoints
{
    /// <summary>The live API.</summary>
    internal const string LiveBaseAddress = "https://api.moolre.com";

    /// <summary>The sandbox API.</summary>
    internal const string SandboxBaseAddress = "https://sandbox.moolre.com";

    // Shared paths (several operations use the same path and differ only by the "type" field).
    private const string AccountCreatePath = "/open/account/create";
    private const string AccountStatusPath = "/open/account/status";
    private const string TransactStatusPath = "/open/transact/status";
    private const string SmsStatusPath = "/open/sms/status";
    private const string SmsSendPath = "/open/sms/send";

    /// <summary>Wallet, settings, balance and account transactions.</summary>
    internal static class Accounts
    {
        internal static readonly MoolreEndpoint CreateSandboxAccount = Define(nameof(Accounts), AccountCreatePath, 1, CredentialKind.User, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint Update = Define(nameof(Accounts), "/open/account/update", 1, CredentialKind.Private, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint GetStatus = Define(nameof(Accounts), AccountStatusPath, 1, CredentialKind.Private, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint ListTransactions = Define(nameof(Accounts), AccountStatusPath, 2, CredentialKind.Private, isSafeToRepeat: true);
    }

    /// <summary>Name validation, payouts and wallet-to-wallet transfers.</summary>
    internal static class Transfers
    {
        internal static readonly MoolreEndpoint ValidateName = Define(nameof(Transfers), "/open/transact/validate", 1, CredentialKind.Either, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint Send = Define(nameof(Transfers), "/open/transact/transfer", 1, CredentialKind.Private, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint GetStatus = Define(nameof(Transfers), TransactStatusPath, 1, CredentialKind.Either, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint SendInternal = Define(nameof(Transfers), "/open/transact/internal", 1, CredentialKind.Private, isSafeToRepeat: false);
    }

    /// <summary>Collections, payment links, Payment IDs and bank collections.</summary>
    internal static class Payments
    {
        internal static readonly MoolreEndpoint RequestMobileMoney = Define(nameof(Payments), "/open/transact/payment", 1, CredentialKind.Public, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint GetStatus = Define(nameof(Payments), TransactStatusPath, 1, CredentialKind.Public, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint CreatePaymentLink = Define(nameof(Payments), "/embed/link", 1, CredentialKind.Public, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint CreatePaymentId = Define(nameof(Payments), AccountCreatePath, 2, CredentialKind.Public, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint CreateBankAccount = Define(nameof(Payments), AccountCreatePath, 9, CredentialKind.Public, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint ListBankAccounts = Define(nameof(Payments), TransactStatusPath, 3, CredentialKind.Public, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint GetPaymentAccount = Define(nameof(Payments), TransactStatusPath, 2, CredentialKind.Public, isSafeToRepeat: true);
    }

    /// <summary>Airtime and data sales (they need a Sell on Our Platform service).</summary>
    internal static class Sales
    {
        internal static readonly MoolreEndpoint SendAirtime = Define(nameof(Sales), "/open/sales/airtime", 1, CredentialKind.Private, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint SendData = Define(nameof(Sales), "/open/sales/data", 1, CredentialKind.Private, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint ListBundles = Define(nameof(Sales), "/open/sales/bundles", null, CredentialKind.Private, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint GetStatus = Define(nameof(Sales), "/open/sales/status", 1, CredentialKind.Private, isSafeToRepeat: true);
    }

    /// <summary>SMS sending and sender IDs.</summary>
    internal static class Sms
    {
        internal static readonly MoolreEndpoint Send = Define(nameof(Sms), SmsSendPath, 1, CredentialKind.Sms, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint SendSingle = Define(nameof(Sms), SmsSendPath, 1, CredentialKind.Sms, isSafeToRepeat: false, HttpMethod.Get);
        internal static readonly MoolreEndpoint GetStatus = Define(nameof(Sms), SmsStatusPath, 5, CredentialKind.Sms, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint CreateSenderIds = Define(nameof(Sms), "/open/sms/query", 3, CredentialKind.Sms, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint GetSenderIdStatus = Define(nameof(Sms), SmsStatusPath, 1, CredentialKind.Sms, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint ListSenderIds = Define(nameof(Sms), SmsStatusPath, 7, CredentialKind.Sms, isSafeToRepeat: true);
        internal static readonly MoolreEndpoint ApproveSenderIds = Define(nameof(Sms), SmsStatusPath, 6, CredentialKind.Sms, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint GetBalance = Define(nameof(Sms), SmsStatusPath, 2, CredentialKind.Sms, isSafeToRepeat: true);
    }

    /// <summary>WhatsApp templates and messages (they have no <c>type</c> field).</summary>
    internal static class WhatsApp
    {
        internal static readonly MoolreEndpoint GetTemplatePage = Define(nameof(WhatsApp), "/open/whatsapp/template", null, CredentialKind.WhatsApp, isSafeToRepeat: true, HttpMethod.Get);
        internal static readonly MoolreEndpoint Send = Define(nameof(WhatsApp), "/open/whatsapp/send", null, CredentialKind.WhatsApp, isSafeToRepeat: false);
        internal static readonly MoolreEndpoint GetStatus = Define(nameof(WhatsApp), "/open/whatsapp/status", null, CredentialKind.WhatsApp, isSafeToRepeat: true);
    }

    /// <summary>Reference data that needs no credentials.</summary>
    internal static class ReferenceData
    {
        internal static readonly MoolreEndpoint GetBanks = Define(nameof(ReferenceData), "/open/transact/data", null, CredentialKind.None, isSafeToRepeat: true, HttpMethod.Get);
    }

    // The operation name comes from the field that calls this (CallerMemberName), so it cannot drift from the code.
    private static MoolreEndpoint Define(
        string area,
        string path,
        int? type,
        CredentialKind credential,
        bool isSafeToRepeat,
        HttpMethod? method = null,
        [CallerMemberName] string operation = "") =>
        new($"{area}.{operation}Async", method ?? HttpMethod.Post, path, type, credential, isSafeToRepeat);

    /// <summary>All operations, for validation and tests.</summary>
    internal static IReadOnlyList<MoolreEndpoint> All { get; } =
    [
        Accounts.CreateSandboxAccount, Accounts.Update, Accounts.GetStatus, Accounts.ListTransactions,
        Transfers.ValidateName, Transfers.Send, Transfers.GetStatus, Transfers.SendInternal,
        Payments.RequestMobileMoney, Payments.GetStatus, Payments.CreatePaymentLink, Payments.CreatePaymentId,
        Payments.CreateBankAccount, Payments.ListBankAccounts, Payments.GetPaymentAccount,
        Sales.SendAirtime, Sales.SendData, Sales.ListBundles, Sales.GetStatus,
        Sms.Send, Sms.SendSingle, Sms.GetStatus, Sms.CreateSenderIds, Sms.GetSenderIdStatus,
        Sms.ListSenderIds, Sms.ApproveSenderIds, Sms.GetBalance,
        WhatsApp.GetTemplatePage, WhatsApp.Send, WhatsApp.GetStatus,
        ReferenceData.GetBanks,
    ];
}
