using System.Net;
using Xunit;

namespace Zhuwa.Moolre;

/// <summary>Tests that every exception keeps what it is given, through every constructor, so callers can always read the details.</summary>
public sealed class ExceptionTests
{
    private static readonly InvalidOperationException Cause = new("cause");

    [Fact]
    public void StandardConstructors_OnEveryException_KeepTheMessageAndCause()
    {
        Exception[] exceptions =
        [
            new MoolreException("m", Cause),
            new MoolreApiException("m", Cause),
            new MoolreAuthenticationException("m", Cause),
            new MoolreDuplicateReferenceException("m", Cause),
            new MoolreResponseException("m", Cause),
            new MoolreOutcomeUncertainException("m", Cause),
        ];

        Assert.All(exceptions, e =>
        {
            Assert.Equal("m", e.Message);
            Assert.Same(Cause, e.InnerException);
            Assert.IsAssignableFrom<MoolreException>(e);
        });
    }

    [Fact]
    public void ParameterlessAndMessageConstructors_Work()
    {
        Assert.Equal("m", new MoolreException("m").Message);
        Assert.NotNull(new MoolreException().Message);
        Assert.Equal("m", new MoolreApiException("m").Message);
        Assert.Empty(new MoolreApiException().Messages);
        Assert.Equal("m", new MoolreAuthenticationException("m").Message);
        Assert.Equal("m", new MoolreDuplicateReferenceException("m").Message);
        Assert.NotNull(new MoolreAuthenticationException().Message);
        Assert.NotNull(new MoolreDuplicateReferenceException().Message);
        Assert.Equal("m", new MoolreResponseException("m").Message);
        Assert.Equal(default, new MoolreResponseException().HttpStatusCode);
        Assert.Equal(string.Empty, new MoolreOutcomeUncertainException().Operation);
        Assert.Null(new MoolreOutcomeUncertainException("m").ExternalReference);
    }

    [Fact]
    public void ApiException_PublicConstructor_KeepsTheCodeAndStatusAndHasNoMessagesOrDetail()
    {
        var exception = new MoolreAuthenticationException("rejected", MoolreResponseCode.AuthenticationError, HttpStatusCode.Unauthorized);

        Assert.Equal(MoolreResponseCode.AuthenticationError, exception.Code);
        Assert.Equal(HttpStatusCode.Unauthorized, exception.HttpStatusCode);
        Assert.Empty(exception.Messages);
        Assert.Null(exception.Detail);
        Assert.IsAssignableFrom<MoolreApiException>(exception);
    }

    [Fact]
    public void DuplicateReferenceException_IsAnApiException()
    {
        var exception = new MoolreDuplicateReferenceException("used", MoolreResponseCode.PayoutDuplicateReference, HttpStatusCode.BadRequest);

        Assert.Equal(MoolreResponseCode.PayoutDuplicateReference, exception.Code);
        Assert.IsAssignableFrom<MoolreApiException>(exception);
    }

    [Fact]
    public void ResponseException_KeepsTheStatusAndCause()
    {
        var exception = new MoolreResponseException("bad body", HttpStatusCode.BadGateway, Cause);

        Assert.Equal(HttpStatusCode.BadGateway, exception.HttpStatusCode);
        Assert.Same(Cause, exception.InnerException);
    }

    [Fact]
    public void OutcomeUncertainException_KeepsTheOperationReferenceAndCause()
    {
        var exception = new MoolreOutcomeUncertainException("unknown", "Transfers.SendAsync", "T-1", Cause);

        Assert.Equal("Transfers.SendAsync", exception.Operation);
        Assert.Equal("T-1", exception.ExternalReference);
        Assert.Same(Cause, exception.InnerException);
    }
}
