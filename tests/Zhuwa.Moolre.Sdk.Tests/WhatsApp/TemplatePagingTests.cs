using Xunit;
using Zhuwa.Moolre.WhatsApp;

namespace Zhuwa.Moolre;

/// <summary>Tests reading WhatsApp templates one page at a time and walking every page by cursor.</summary>
public sealed class TemplatePagingTests
{
    // One page holding one template, with the given cursor for the next page (null on the last page).
    private static string Page(string templateId, string? next) =>
        """{"status":1,"code":"WAS200","message":"ok","data":[{"id":""" + "\"" + templateId + "\"" +
        ""","name":"t","language":"en","status":"APPROVED","message":"Hi","placeholders":["1"]}],"next":""" +
        (next is null ? "null" : "\"" + next + "\"") + "}";

    [Fact]
    public async Task GetTemplatePageAsync_ForTheDocumentedExample_ReadsTheTemplateAndTheCursor()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("get-templates/response-200-approved-success.json"));

        var page = await sdk.WhatsApp.GetTemplatePageAsync();

        var template = Assert.Single(page.Templates);
        Assert.Equal("1218877297066056", template.Id);
        Assert.Equal("promotion", template.Name);
        Assert.Equal(WhatsAppTemplateStatus.Approved, template.Status);
        Assert.StartsWith("Hello {{1}},", template.Body, StringComparison.Ordinal);
        Assert.Equal(["1", "2", "3"], template.Placeholders);
        Assert.Equal("opaque-cursor-for-the-next-page", page.NextCursor);
        Assert.Equal(HttpMethod.Get, sdk.Handler.Request!.Method);
        Assert.Equal(string.Empty, sdk.Handler.Request.RequestUri!.Query);
        Assert.Equal("whatsapp-vas-key", sdk.Handler.Request.Headers.GetValues("X-API-VASKEY").Single());
    }

    [Theory]
    [InlineData("get-templates/response-200-pending-success.json", "PENDING")]
    [InlineData("get-templates/response-200-rejected-success.json", "REJECTED")]
    public async Task GetTemplatePageAsync_OnTheLastPage_HasNoCursor(string fixture, string status)
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture(fixture));

        var page = await sdk.WhatsApp.GetTemplatePageAsync();

        Assert.Null(page.NextCursor);
        Assert.Equal(status, page.Templates[0].Status.Value);
    }

    [Fact]
    public async Task GetTemplatePageAsync_WithACursor_SendsItEscaped()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("get-templates/response-200-pending-success.json"));

        await sdk.WhatsApp.GetTemplatePageAsync(new GetWhatsAppTemplatesRequest { Cursor = "a b&c" });

        Assert.Equal("?next=a%20b%26c", sdk.Handler.Request!.RequestUri!.Query);
    }

    [Fact]
    public async Task GetAllTemplatesAsync_FollowsTheCursorUntilTheLastPage()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningInOrder(Page("1", "c2"), Page("2", "c3"), Page("3", null)));

        var ids = new List<string>();
        await foreach (var template in sdk.WhatsApp.GetAllTemplatesAsync())
        {
            ids.Add(template.Id);
        }

        Assert.Equal(["1", "2", "3"], ids);
        Assert.Equal(["", "?next=c2", "?next=c3"], sdk.Handler.RequestUris.Select(u => u.Query));
    }

    [Fact]
    public async Task GetAllTemplatesAsync_WhenMoolreRepeatsACursor_StopsWithAnErrorInsteadOfLoopingForever()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningInOrder(Page("1", "same"), Page("2", "same")));

        await Assert.ThrowsAsync<MoolreResponseException>(async () =>
        {
            await foreach (var _ in sdk.WhatsApp.GetAllTemplatesAsync())
            {
            }
        });

        Assert.Equal(2, sdk.Handler.CallCount);
    }

    [Fact]
    public async Task GetAllTemplatesAsync_FetchesTheNextPageOnlyWhenItIsReached()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningInOrder(Page("1", "c2"), Page("2", null)));

        await foreach (var _ in sdk.WhatsApp.GetAllTemplatesAsync())
        {
            break;
        }

        Assert.Equal(1, sdk.Handler.CallCount);
    }
}
