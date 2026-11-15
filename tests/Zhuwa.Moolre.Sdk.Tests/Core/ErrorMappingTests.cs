using System.Net;
using Xunit;
using Zhuwa.Moolre.Core;

namespace Zhuwa.Moolre;

/// <summary>Tests how Moolre's failure codes become exceptions: authentication, duplicate reference and other failures.</summary>
public sealed class ErrorMappingTests
{
    private static Task<ResponseEnvelope> Send(TestTransport sdk, MoolreEndpoint endpoint, string? externalReference = null) =>
        sdk.Transport.SendAsync(endpoint, writer => writer.WriteString("accountnumber", "1"), query: null, externalReference, CancellationToken.None);

    [Fact]
    public async Task SendAsync_ForAnAuthenticationCode_ThrowsAuthenticationException()
    {
        var sdk = TestTransport.Create(StubHttpHandler.ReturningFixture("account-status/response-401-authentication-error.json", HttpStatusCode.Unauthorized));

        var exception = await Assert.ThrowsAsync<MoolreAuthenticationException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));

        Assert.Equal(MoolreResponseCode.ApiAccessNotActivated, exception.Code);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.HttpStatusCode);
        Assert.Equal("all", exception.Detail);
    }

    [Fact]
    public async Task SendAsync_ForADuplicateReferenceCode_ThrowsDuplicateReferenceException()
    {
        var handler = StubHttpHandler.Returning("""{"status":0,"code":"APY18","message":"Duplicate externalref","data":null,"go":null}""");
        var sdk = TestTransport.Create(handler);

        var exception = await Assert.ThrowsAsync<MoolreDuplicateReferenceException>(() => Send(sdk, MoolreEndpoints.Transfers.Send, "T-1"));

        Assert.Equal(MoolreResponseCode.PayoutDuplicateReference, exception.Code);
    }

    [Fact]
    public void EnsureSuccess_ForStatusZero_ThrowsApiExceptionWithTheCode()
    {
        var envelope = ResponseEnvelope.Parse(
            """{"status":0,"code":"WCU01","message":"Invalid account number","data":"accountnumber"}"""u8, HttpStatusCode.OK);

        var exception = Assert.Throws<MoolreApiException>(() => MoolreTransport.EnsureSuccess(envelope));

        Assert.Equal(MoolreResponseCode.UpdateInvalidAccountNumber, exception.Code);
        Assert.Equal("accountnumber", exception.Detail);
    }

    [Theory]
    [InlineData("AIN01")]
    [InlineData("AIN03")]
    [InlineData("AIN04")]
    [InlineData("IN01")]
    [InlineData("ASL00")]
    [InlineData("ASS00")]
    public async Task SendAsync_ForEveryAuthenticationCode_ThrowsAuthenticationException(string code)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning($$"""{"status":0,"code":"{{code}}","message":"no"}"""));

        var exception = await Assert.ThrowsAsync<MoolreAuthenticationException>(() => Send(sdk, MoolreEndpoints.Accounts.GetStatus));

        Assert.Equal(code, exception.Code.Value);
    }

    [Theory]
    [InlineData("TP13")]
    [InlineData("TR013")]
    [InlineData("APY18")]
    [InlineData("ASL08")]
    [InlineData("INP02")]
    public async Task SendAsync_ForEveryDuplicateReferenceCode_ThrowsDuplicateReferenceException(string code)
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning($$"""{"status":0,"code":"{{code}}","message":"duplicate"}"""));

        var exception = await Assert.ThrowsAsync<MoolreDuplicateReferenceException>(() => Send(sdk, MoolreEndpoints.Transfers.Send, "T-1"));

        Assert.Equal(code, exception.Code.Value);
    }

    [Fact]
    public async Task SendAsync_ForAnUnknownFailureCode_ReturnsTheEnvelopeForTheClientToDecide()
    {
        var sdk = TestTransport.Create(StubHttpHandler.Returning("""{"status":0,"code":"AVD02","message":"Name not found"}"""));

        var envelope = await Send(sdk, MoolreEndpoints.Transfers.ValidateName);

        Assert.False(envelope.IsSuccess);
        Assert.Equal(MoolreResponseCode.NameNotFound, envelope.Code);
    }
}
