using System.Net;
using Xunit;
using Zhuwa.Moolre.WhatsApp;

namespace Zhuwa.Moolre;

public sealed class WhatsAppTests
{
    private static WhatsAppSendResult CreateResult(string? warning, params WhatsAppRejectedMessage[] notSent) =>
        new(MoolreResponseCode.WhatsAppSuccess, "success", ["success"], HttpStatusCode.OK, warning, notSent);

    [Fact]
    public void AllSent_WithNoWarningAndNothingUnsent_IsTrue()
    {
        var result = CreateResult(warning: null);

        Assert.True(result.AllSent);
    }

    [Fact]
    public void AllSent_WhenMoolreWarnsAboutDuplicateReferences_IsFalse()
    {
        var unsent = new WhatsAppRejectedMessage("233531419000", "REF-1");

        var result = CreateResult("Some ref are not unique, corresponding messages will not be sent", unsent);

        Assert.False(result.AllSent);
        Assert.Single(result.NotSent);
    }

    [Fact]
    public void SendRequest_ByDefault_HasNoMessages()
    {
        var request = new SendWhatsAppMessagesRequest { TemplateName = "promotion", Language = "en" };

        Assert.Empty(request.Messages);
    }

    [Fact]
    public void Message_ByDefault_HasNoPlaceholders()
    {
        var message = new WhatsAppMessage { Recipient = "233531419000" };

        Assert.Empty(message.Placeholders);
        Assert.Null(message.Reference);
    }

    [Fact]
    public void TemplateStatus_ForUnknownValues_KeepsTheTextExactly()
    {
        WhatsAppTemplateStatus status = "PAUSED";

        Assert.Equal("PAUSED", status.Value);
        Assert.NotEqual(WhatsAppTemplateStatus.Approved, status);
    }

    [Fact]
    public void TemplateStatus_WellKnownValues_MatchMoolresText()
    {
        Assert.Equal("APPROVED", WhatsAppTemplateStatus.Approved.Value);
        Assert.Equal("REJECTED", WhatsAppTemplateStatus.Rejected.Value);
        Assert.Equal("PENDING", WhatsAppTemplateStatus.Pending.Value);
    }

    // ---- Send and status through the stub handler

    private static SendWhatsAppMessagesRequest Promotion(params WhatsAppMessage[] messages)
    {
        var request = new SendWhatsAppMessagesRequest { TemplateName = "promotion", Language = "en" };
        foreach (var message in messages)
        {
            request.Messages.Add(message);
        }

        return request;
    }

    private static WhatsAppMessage To(string recipient, string? reference, params string[] placeholders)
    {
        var message = new WhatsAppMessage { Recipient = recipient, Reference = reference };
        foreach (var value in placeholders)
        {
            message.Placeholders.Add(value);
        }

        return message;
    }

    [Fact]
    public async Task SendAsync_ForTheDocumentedSuccess_ReportsAllSentAndSendsTheTemplateBody()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-message/response-200-success-success.json"));

        var result = await sdk.WhatsApp.SendAsync(Promotion(To("233531419000", "REF-1", "Ama", "loans", "Zhuwa")));

        Assert.True(result.AllSent);
        Assert.Null(result.Warning);
        Assert.True(System.Text.Json.Nodes.JsonNode.DeepEquals(
            System.Text.Json.Nodes.JsonNode.Parse("""{"template_name":"promotion","language":"en","messages":[{"recipient":"233531419000","ref":"REF-1","placeholders":["Ama","loans","Zhuwa"]}]}"""),
            System.Text.Json.Nodes.JsonNode.Parse(sdk.Handler.RequestBody!)));
        Assert.Equal("whatsapp-vas-key", sdk.Handler.Request!.Headers.GetValues("X-API-VASKEY").Single());
    }

    [Fact]
    public async Task SendAsync_WhenSomeReferencesAreNotUnique_ReportsTheWarningAndTheUnsentMessages()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-message/response-200-duplicate-refs-warning.json"));

        var result = await sdk.WhatsApp.SendAsync(Promotion(To("233531419000", "R"), To("233531419011", "R")));

        Assert.False(result.AllSent);
        Assert.Equal("Some ref are not unique, corresponding messages will not be sent", result.Warning);
        Assert.Equal(2, result.NotSent.Count);
        Assert.Equal("233531419011", result.NotSent[1].Recipient);
        Assert.Equal("879883HGU334553GF887799HF20006", result.NotSent[1].Reference);
    }

    [Fact]
    public async Task SendAsync_WithInsufficientBalance_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-message/response-401-insufficient-balance-insufficient-balance.json", HttpStatusCode.Unauthorized));

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.WhatsApp.SendAsync(Promotion(To("233531419000", "R"))));

        Assert.Equal(MoolreResponseCode.WhatsAppInsufficientBalance, exception.Code);
    }

    [Fact]
    public async Task SendAsync_WhenTheConnectionFails_ThrowsUncertainAndSendsOnce()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Throwing(new HttpRequestException("reset")));

        var exception = await Assert.ThrowsAsync<MoolreOutcomeUncertainException>(() => sdk.WhatsApp.SendAsync(Promotion(To("233531419000", "R"))));

        Assert.Equal("WhatsApp.SendAsync", exception.Operation);
        Assert.Equal(1, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithNoMessagesOrNoTemplate_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-message/response-200-success-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.WhatsApp.SendAsync(Promotion()));
        await Assert.ThrowsAsync<ArgumentException>(() =>
            sdk.WhatsApp.SendAsync(new SendWhatsAppMessagesRequest { TemplateName = " ", Language = "en" }));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task SendAsync_WithoutTheWhatsAppKey_FailsBeforeAnyRequest()
    {
        var options = TestTransport.SandboxOptions();
        options.WhatsAppVasKey = null;
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("send-message/response-200-success-success.json"), options);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sdk.WhatsApp.SendAsync(Promotion(To("1", "R"))));

        Assert.Contains("WhatsAppVasKey", exception.Message, StringComparison.Ordinal);
        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task GetStatusAsync_ForTheDocumentedExample_ReadsEachStatusAsText()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("message-status/response-200-success-success.json"));
        var request = new GetWhatsAppMessageStatusRequest();
        request.References.Add("879883HGUGF45583499HF2089001");
        request.References.Add("879883HGUGF45583499HF2089005");

        var result = await sdk.WhatsApp.GetStatusAsync(request);

        Assert.Equal(WhatsAppDeliveryStatus.Read, result.Statuses[0].Status);
        Assert.Equal(WhatsAppDeliveryStatus.Accepted, result.Statuses[1].Status);
        Assert.Equal("""{"ref":["879883HGUGF45583499HF2089001","879883HGUGF45583499HF2089005"]}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task GetStatusAsync_WithNoReferences_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("message-status/response-200-success-success.json"));

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.WhatsApp.GetStatusAsync(new GetWhatsAppMessageStatusRequest()));

        Assert.Equal(0, sdk.Handler.CallCount);
    }
}
