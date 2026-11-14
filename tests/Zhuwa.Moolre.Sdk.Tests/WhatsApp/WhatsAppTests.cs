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
}
