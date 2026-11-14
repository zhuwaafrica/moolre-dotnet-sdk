using Xunit;

namespace Zhuwa.Moolre;

public sealed class MoolreOptionsTests
{
    [Fact]
    public void Environment_ByDefault_IsSandbox()
    {
        var options = new MoolreOptions();

        Assert.Equal(MoolreEnvironment.Sandbox, options.Environment);
    }

    [Fact]
    public void Timeout_ByDefault_IsThirtySeconds()
    {
        var options = new MoolreOptions();

        Assert.Equal(TimeSpan.FromSeconds(30), options.Timeout);
    }

    [Fact]
    public void Credentials_ByDefault_AreNotSet()
    {
        var options = new MoolreOptions();

        Assert.Null(options.ApiUser);
        Assert.Null(options.PrivateKey);
        Assert.Null(options.PublicKey);
        Assert.Null(options.SmsVasKey);
        Assert.Null(options.WhatsAppVasKey);
        Assert.Null(options.AccountNumber);
        Assert.Null(options.BaseAddress);
    }

    [Fact]
    public void MoolreEnvironment_SandboxIsTheZeroValue()
    {
        Assert.Equal(0, (int)MoolreEnvironment.Sandbox);
    }
}
