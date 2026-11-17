using Xunit;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

public sealed class EndpointTableTests
{
    private static readonly string[] OperationsThatMoveMoneyOrSendMessages =
    [
        "Transfers.SendAsync",
        "Transfers.SendInternalAsync",
        "Payments.RequestMobileMoneyAsync",
        "Sales.SendAirtimeAsync",
        "Sales.SendDataAsync",
        "Sms.SendAsync",
        "Sms.SendSingleAsync",
        "WhatsApp.SendAsync",
    ];

    [Fact]
    public void All_Always_ListsEveryDocumentedOperation()
    {
        const int DocumentedOperationCount = 31;

        var actual = MoolreEndpoints.All.Count;

        Assert.Equal(DocumentedOperationCount, actual);
    }

    [Fact]
    public void All_Always_HasUniqueOperationNames()
    {
        var names = MoolreEndpoints.All.Select(e => e.Name).ToList();

        Assert.Equal(names.Count, names.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void All_Always_StartsEveryPathWithASlash()
    {
        Assert.All(MoolreEndpoints.All, e => Assert.StartsWith("/", e.Path, StringComparison.Ordinal));
    }

    [Fact]
    public void All_Always_NeverRepeatsTheSameRouteTypeAndCredential()
    {
        // Payment status and transfer status are the same wire request; only the credential differs.
        var keys = MoolreEndpoints.All
            .Select(e => $"{e.Method} {e.Path} type={e.Type} credential={e.Credential}")
            .ToList();

        Assert.Equal(keys.Count, keys.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData("Transfers.SendAsync")]
    [InlineData("Transfers.SendInternalAsync")]
    [InlineData("Payments.RequestMobileMoneyAsync")]
    [InlineData("Sales.SendAirtimeAsync")]
    [InlineData("Sales.SendDataAsync")]
    [InlineData("Sms.SendAsync")]
    [InlineData("Sms.SendSingleAsync")]
    [InlineData("WhatsApp.SendAsync")]
    public void Endpoint_ThatMovesMoneyOrSendsAMessage_IsNeverSafeToRepeat(string operation)
    {
        var endpoint = MoolreEndpoints.All.Single(e => e.Name == operation);

        Assert.False(endpoint.IsSafeToRepeat);
    }

    [Fact]
    public void All_ReadOnlyLookups_AreSafeToRepeat()
    {
        string[] lookups =
        [
            "Accounts.GetStatusAsync", "Accounts.ListTransactionsAsync", "Transfers.ValidateNameAsync",
            "Transfers.GetStatusAsync", "Payments.GetStatusAsync", "Payments.ListBankAccountsAsync",
            "Payments.GetPaymentAccountAsync", "Sales.ListBundlesAsync", "Sales.GetStatusAsync",
            "Sms.GetStatusAsync", "Sms.GetSenderIdStatusAsync", "Sms.ListSenderIdsAsync", "Sms.GetBalanceAsync",
            "WhatsApp.GetTemplatePageAsync", "WhatsApp.GetStatusAsync", "ReferenceData.GetBanksAsync",
        ];

        var safe = MoolreEndpoints.All.Where(e => e.IsSafeToRepeat).Select(e => e.Name).Order(StringComparer.Ordinal);

        Assert.Equal(lookups.Order(StringComparer.Ordinal), safe);
    }

    [Fact]
    public void All_Always_ListsEveryMoneyOrMessageOperationAsUnsafe()
    {
        var unsafeOperations = MoolreEndpoints.All.Where(e => !e.IsSafeToRepeat).Select(e => e.Name);

        Assert.All(OperationsThatMoveMoneyOrSendMessages, op => Assert.Contains(op, unsafeOperations));
    }

    [Fact]
    public void Credentials_ForTheSmsAndWhatsAppAreas_AreTheirOwnVasKeys()
    {
        var sms = MoolreEndpoints.All.Where(e => e.Name.StartsWith("Sms.", StringComparison.Ordinal));
        var whatsApp = MoolreEndpoints.All.Where(e => e.Name.StartsWith("WhatsApp.", StringComparison.Ordinal));

        Assert.All(sms, e => Assert.Equal(CredentialKind.Sms, e.Credential));
        Assert.All(whatsApp, e => Assert.Equal(CredentialKind.WhatsApp, e.Credential));
    }

    [Fact]
    public void Credential_ForReferenceData_IsNone()
    {
        var banks = MoolreEndpoints.ReferenceData.GetBanks;

        Assert.Equal(CredentialKind.None, banks.Credential);
    }

    [Fact]
    public void Credential_ForSandboxAccountCreation_IsTheUserOnly()
    {
        // Moolre's sandbox Create Account page: X-API-KEY is "Optional in Sandbox. Omit placeholder keys."
        Assert.Equal(CredentialKind.User, MoolreEndpoints.Accounts.CreateSandboxAccount.Credential);
    }

    [Fact]
    public void Transfers_Send_NeedsThePrivateKeyAndCollectionsThePublicKey()
    {
        Assert.Equal(CredentialKind.Private, MoolreEndpoints.Transfers.Send.Credential);
        Assert.Equal(CredentialKind.Public, MoolreEndpoints.Payments.RequestMobileMoney.Credential);
    }

    [Fact]
    public void BaseAddresses_Always_AreHttps()
    {
        Assert.StartsWith("https://", MoolreEndpoints.LiveBaseAddress, StringComparison.Ordinal);
        Assert.StartsWith("https://", MoolreEndpoints.SandboxBaseAddress, StringComparison.Ordinal);
    }
}
