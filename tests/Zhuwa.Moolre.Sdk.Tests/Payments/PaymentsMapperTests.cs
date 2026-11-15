using System.Net;
using System.Text;
using Xunit;
using Zhuwa.Moolre.Core;
using Zhuwa.Moolre.Payments;

namespace Zhuwa.Moolre;

/// <summary>Tests the payment mappers on their own with odd shapes Moolre might send, with no HTTP.</summary>
public sealed class PaymentsMapperTests
{
    private static ResponseEnvelope Envelope(string json) => ResponseEnvelope.Parse(Encoding.UTF8.GetBytes(json), HttpStatusCode.OK);

    [Fact]
    public void ToPaymentAccountLookupResult_WhenTransactionsIsMissingOrNull_ReturnsAnEmptyList()
    {
        var missing = Envelope("""{"status":1,"code":"AC03","data":{"account":null}}""").ToPaymentAccountLookupResult();
        var nullValue = Envelope("""{"status":1,"code":"AC03","data":{"account":null,"transactions":null}}""").ToPaymentAccountLookupResult();

        Assert.Empty(missing.Transactions);
        Assert.Empty(nullValue.Transactions);
    }

    [Fact]
    public void ToPaymentAccountLookupResult_SkipsArrayEntriesThatAreNotObjects()
    {
        var result = Envelope("""{"status":1,"code":"AC01","data":{"transactions":[1,{"txstatus":1},"x"]}}""").ToPaymentAccountLookupResult();

        Assert.Single(result.Transactions);
    }

    [Fact]
    public void ToBankAccountListResult_WhenTheCountIsMissing_UsesTheNumberOfAccounts()
    {
        var result = Envelope("""{"status":1,"code":"AC01","data":{"accounts":[{"accountno":"1"},{"accountno":"2"}]}}""").ToBankAccountListResult();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ToBankAccountResult_WhenMoolreDoesNotEchoTheReference_UsesTheRequestsReference()
    {
        var result = Envelope("""{"status":1,"code":"AD19","data":{"accountno":"1"}}""").ToBankAccountResult("REQ-REF");

        Assert.Equal("REQ-REF", result.ExternalReference);
    }

    [Fact]
    public void ToMobileMoneyResult_WhenPromptSentHasNoStringData_LeavesTheRequestIdEmpty()
    {
        var result = Envelope("""{"status":1,"code":"TR099","data":null}""").ToMobileMoneyResult();

        Assert.Equal(MobileMoneyOutcome.PromptSent, result.Outcome);
        Assert.Null(result.RequestId);
    }
}
