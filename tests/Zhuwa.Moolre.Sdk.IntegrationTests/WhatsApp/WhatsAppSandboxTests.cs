using Xunit;

namespace Zhuwa.Moolre;

/// <summary>WhatsApp against the real sandbox, with the WhatsApp service's own key. Read-only, so nothing is sent.</summary>
public sealed class WhatsAppSandboxTests
{
    [SandboxFact(SandboxNeeds.WhatsAppKey)]
    public async Task GetAllTemplatesAsync_WalksEveryPageAndEachTemplateHasANameAndStatus()
    {
        using var sandbox = new SandboxClient();

        await foreach (var template in sandbox.Moolre.WhatsApp.GetAllTemplatesAsync(TestContext.Current.CancellationToken))
        {
            Assert.False(string.IsNullOrWhiteSpace(template.Name));
            Assert.False(string.IsNullOrWhiteSpace(template.Status.Value));
        }
    }
}
