using System.Net;
using System.Text.Json.Nodes;
using Xunit;
using Zhuwa.Moolre.Sms;

namespace Zhuwa.Moolre;

/// <summary>Tests registering, checking, listing and approving sender IDs through the stub handler.</summary>
public sealed class SenderIdTests
{
    private static void AssertBody(string expected, string? actual) =>
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse(expected), JsonNode.Parse(actual!)), actual);

    [Fact]
    public async Task CreateSenderIdsAsync_SendsTheSenderIdsAndReturnsTheAcceptance()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-sender-id/response-200-success.json"));
        var request = new CreateSenderIdsRequest();
        request.SenderIds.Add(new SenderIdRegistration { SenderId = "Zhuwa" });
        request.SenderIds.Add(new SenderIdRegistration { SenderId = "ZhuwaPay", Approve = true });

        var result = await sdk.Sms.CreateSenderIdsAsync(request);

        Assert.Equal(MoolreResponseCode.SenderIdsCreated, result.Code);
        AssertBody("""{"type":3,"senderids":[{"senderid":"Zhuwa"},{"senderid":"ZhuwaPay","approve":true}]}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task CreateSenderIdsAsync_WithoutPermissionToApprove_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-sender-id/response-400-permission-denied.json", HttpStatusCode.BadRequest));
        var request = new CreateSenderIdsRequest();
        request.SenderIds.Add(new SenderIdRegistration { SenderId = "Zhuwa", Approve = true });

        var exception = await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Sms.CreateSenderIdsAsync(request));

        Assert.Equal(MoolreResponseCode.SenderIdNoPermission, exception.Code);
    }

    [Fact]
    public async Task CreateSenderIdsAsync_WithASenderIdOver11Characters_ThrowsAndSendsNothing()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("create-sender-id/response-200-success.json"));
        var request = new CreateSenderIdsRequest();
        request.SenderIds.Add(new SenderIdRegistration { SenderId = "TwelveChars1" });

        await Assert.ThrowsAsync<ArgumentException>(() => sdk.Sms.CreateSenderIdsAsync(request));

        Assert.Equal(0, sdk.Handler.CallCount);
    }

    [Theory]
    [InlineData("sender-id-status/response-200-success-success.json", "SmartSMS", "Approved")]
    [InlineData("sender-id-status/response-200-pending-pending.json", "Dummy", "Pending")]
    [InlineData("sender-id-status/response-200-rejected-rejected.json", "Momo", "Rejected")]
    public async Task GetSenderIdStatusAsync_ForEachDocumentedApproval_ReadsIt(string fixture, string senderId, string approval)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture(fixture));

        var result = await sdk.Sms.GetSenderIdStatusAsync(new GetSenderIdStatusRequest { SenderId = senderId });

        Assert.Equal(senderId, result.SenderId);
        Assert.Equal(new SenderIdApproval(approval), result.Approval);
        Assert.False(result.IsWhitelisted);
        AssertBody($$"""{"type":1,"senderid":"{{senderId}}"}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task ListSenderIdsAsync_ForTheDocumentedExample_ReadsNumericIdsAsText()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("list-sender-ids/response-200-success.json"));

        var result = await sdk.Sms.ListSenderIdsAsync();

        Assert.Equal(4, result.SenderIds.Count);
        Assert.Equal("4010", result.SenderIds[3].Id);
        Assert.Equal(SenderIdApproval.Rejected, result.SenderIds[3].Approval);
        AssertBody("""{"type":7}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task ApproveSenderIdsAsync_SendsTheDecisionsAsNumbersAndReadsTextIds()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("approve-sender-id/response-200-success.json"));
        var request = new ApproveSenderIdsRequest();
        request.Decisions.Add(new SenderIdDecision { SenderId = "Dummy", Decision = SenderIdDecisionKind.Approved });
        request.Decisions.Add(new SenderIdDecision { SenderId = "SmartSMS", Decision = SenderIdDecisionKind.Rejected });

        var result = await sdk.Sms.ApproveSenderIdsAsync(request);

        Assert.Equal("14", result.SenderIds[0].Id);
        Assert.Equal(SenderIdApproval.Approved, result.SenderIds[0].Approval);
        AssertBody("""{"type":6,"senderids":[{"senderid":"Dummy","approve":1},{"senderid":"SmartSMS","approve":2}]}""", sdk.Handler.RequestBody);
    }

    [Fact]
    public async Task ApproveSenderIdsAsync_WithoutPermission_ThrowsApiException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("approve-sender-id/response-400-permission-denied.json", HttpStatusCode.BadRequest));
        var request = new ApproveSenderIdsRequest();
        request.Decisions.Add(new SenderIdDecision { SenderId = "Dummy", Decision = SenderIdDecisionKind.Approved });

        await Assert.ThrowsAsync<MoolreApiException>(() => sdk.Sms.ApproveSenderIdsAsync(request));
    }

    [Fact]
    public async Task SenderIdApproval_ForAValueMoolreAddsLater_KeepsTheTextExactly()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":1,"code":"ASMQ01","data":{"senderid":"X","approval":"Suspended","whitelisted":true}}"""));

        var result = await sdk.Sms.GetSenderIdStatusAsync(new GetSenderIdStatusRequest { SenderId = "X" });

        Assert.Equal("Suspended", result.Approval.Value);
        Assert.True(result.IsWhitelisted);
    }
}
