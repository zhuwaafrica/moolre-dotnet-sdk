using System.Globalization;
using System.Net;
using System.Text.Json.Nodes;
using Xunit;
using Zhuwa.Moolre.Core;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests the transport against the conditions that cause real incidents: every operation's credentials and method, a server culture that
/// writes decimal commas, oversized and error responses, and Moolre answers carried on HTTP error statuses.
/// </summary>
public sealed class TransportHardeningTests
{
    private const string Ok = """{"status":1,"code":"SW01","message":"ok","data":null}""";

    private static MoolreOptions LiveOptions() => new()
    {
        Environment = MoolreEnvironment.Live,
        ApiUser = "user",
        PrivateKey = "private",
        PublicKey = "public",
        SmsVasKey = "sms",
        WhatsAppVasKey = "whatsapp",
        AccountNumber = "1",
    };

    public static TheoryData<string> EveryOperation()
    {
        var data = new TheoryData<string>();
        foreach (var endpoint in MoolreEndpoints.All)
        {
            data.Add(endpoint.Name);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(EveryOperation))]
    public async Task EveryOperation_SendsExactlyItsOwnCredentialsMethodAndType(string operation)
    {
        var endpoint = MoolreEndpoints.All.Single(e => e.Name == operation);
        var sdk = TestTransport.Create(StubHttpHandler.Returning(Ok), LiveOptions());

        await sdk.Transport.SendAsync(endpoint, new JsonObject(), externalReference: null, TestContext.Current.CancellationToken);

        var request = sdk.Handler.Request!;
        string? Header(string name) => request.Headers.TryGetValues(name, out var values) ? values.Single() : null;
        var expected = endpoint.Credential switch
        {
            CredentialKind.Private => ("user", "private", (string?)null, (string?)null),
            CredentialKind.Either => ("user", "private", null, null),
            CredentialKind.Public => ("user", null, "public", null),
            CredentialKind.Sms => (null, null, null, "sms"),
            CredentialKind.WhatsApp => (null, null, null, "whatsapp"),
            _ => ((string?)null, (string?)null, (string?)null, (string?)null),
        };

        Assert.Equal(expected, (Header("X-API-USER"), Header("X-API-KEY"), Header("X-API-PUBKEY"), Header("X-API-VASKEY")));
        Assert.Equal(endpoint.Method, request.Method);
        Assert.Equal(endpoint.Path, request.RequestUri!.AbsolutePath);

        // The type selector goes in the body of a POST and in the query string of a GET, and only when the operation has one.
        var typeSent = endpoint.Method == HttpMethod.Get
            ? request.RequestUri.Query.Contains("type=", StringComparison.Ordinal)
            : JsonNode.Parse(sdk.Handler.RequestBody!)!.AsObject().ContainsKey("type");
        Assert.Equal(endpoint.Type is not null, typeSent);
    }

    [Fact]
    public async Task Amounts_OnAServerWhoseCultureWritesDecimalCommas_AreStillSentWithAPoint()
    {
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        try
        {
            var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));

            await sdk.Transfers.SendAsync(new SendTransferRequest
            {
                Channel = TransferChannel.Mtn,
                Currency = Currency.Ghs,
                Amount = 1234.56m,
                Receiver = "0246798993",
                ExternalReference = "T-CULTURE",
            });

            Assert.Contains("\"amount\":\"1234.56\"", sdk.Handler.RequestBody, StringComparison.Ordinal);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public async Task Responses_OnAServerWhoseCultureWritesDecimalCommas_AreStillReadCorrectly()
    {
        var original = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
        try
        {
            var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("initiate-transfer/response-200-success.json"));

            var result = await sdk.Transfers.SendAsync(new SendTransferRequest
            {
                Channel = TransferChannel.Mtn,
                Currency = Currency.Ghs,
                Amount = 1m,
                Receiver = "0246798993",
                ExternalReference = "T-CULTURE-2",
            });

            Assert.Equal(1.01m, result.AmountWithFee);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public async Task AResponseOver16Megabytes_OnAReadCall_IsRefusedInsteadOfFillingMemory()
    {
        var huge = "{\"status\":1,\"code\":\"SW01\",\"message\":\"" + new string('x', 17 * 1024 * 1024) + "\"}";
        var sdk = TestTransport.Create(StubHttpHandler.Returning(huge));

        await Assert.ThrowsAsync<HttpRequestException>(() => sdk.Accounts.GetStatusAsync());
    }

    [Fact]
    public async Task AResponseOver16Megabytes_OnAMoneyCall_IsAnUnknownOutcome()
    {
        var huge = "{\"status\":1,\"code\":\"OBGH01\",\"message\":\"" + new string('x', 17 * 1024 * 1024) + "\"}";
        var sdk = TestTransport.Create(StubHttpHandler.Returning(huge));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.Transfers.SendAsync(new SendTransferRequest
        {
            Channel = TransferChannel.Mtn,
            Currency = Currency.Ghs,
            Amount = 1m,
            Receiver = "0246798993",
            ExternalReference = "T-HUGE",
        }));

        Assert.Equal("T-HUGE", exception.ExternalReference);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.Unauthorized)]
    public async Task AMoolreAnswerOnAnHttpErrorStatus_IsStillReadByItsCode(HttpStatusCode status)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"AIN01","message":"Authentication Error"}""", status));

        var exception = await Assert.ThrowsAsync<MoolreAuthenticationException>(() => sdk.Accounts.GetStatusAsync());

        Assert.Equal(status, exception.HttpStatusCode);
    }

    [Fact]
    public async Task ASuccessStatusWithAnEmptyBody_OnAReadCall_IsAResponseError()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning(string.Empty, HttpStatusCode.NoContent));

        await Assert.ThrowsAsync<MoolreResponseException>(() => sdk.Accounts.GetStatusAsync());
    }

    [Fact]
    public async Task TextWithAccentsAndEmoji_IsSentAsValidJsonThatReadsBackUnchanged()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-sms/response-200-success.json"));
        var request = new Sms.SendSmsRequest { SenderId = "Zhuwa" };
        request.Messages.Add(new SmsMessage { Recipient = "233241234567", Text = "Akwaaba Adéwálé 👋 \"quoted\" & <tag>" });

        await sdk.Sms.SendAsync(request);

        var sent = JsonNode.Parse(sdk.Handler.RequestBody!)!["messages"]![0]!["message"]!.GetValue<string>();
        Assert.Equal("Akwaaba Adéwálé 👋 \"quoted\" & <tag>", sent);
    }
}
