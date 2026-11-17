using System.Net;
using Xunit;
using Zhuwa.Moolre.Core;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre;

/// <summary>Tests the transfer mappers and the shared transaction mapper on their own, with no HTTP.</summary>
public sealed class TransfersMapperTests
{
    private static ResponseEnvelope Envelope(string json) => ResponseEnvelope.Parse(System.Text.Encoding.UTF8.GetBytes(json), HttpStatusCode.OK);

    private static readonly SendTransferRequest Request = new()
    {
        Channel = TransferChannel.Mtn,
        Currency = Currency.Ghs,
        Amount = 1m,
        Receiver = "0246798993",
        ExternalReference = "REQ-1",
    };

    [Fact]
    public void ToTransferResult_WhenDataIsMissing_FallsBackToTheRequestValues()
    {
        var result = Envelope("""{"status":0,"code":"OTGH02","message":"Pay out in Progress"}""").ToTransferResult(Request);

        Assert.Equal("REQ-1", result.ExternalReference);
        Assert.Equal("0246798993", result.Receiver);
        Assert.Equal(TransactionStatus.Pending, result.Status);
    }

    [Theory]
    [InlineData("0", TransactionStatus.Pending)]
    [InlineData("1", TransactionStatus.Successful)]
    [InlineData("2", TransactionStatus.Failed)]
    [InlineData("\"1\"", TransactionStatus.Successful)]
    [InlineData("9", TransactionStatus.Pending)]
    [InlineData("null", TransactionStatus.Pending)]
    public void ToTransferResult_ReadsTxStatusAsANumberOrAStringAndTreatsUnknownAsPending(string txStatus, TransactionStatus expected)
    {
        var result = Envelope("""{"status":1,"code":"OBGH01","data":{"txstatus":""" + txStatus + "}}").ToTransferResult(Request);

        Assert.Equal(expected, result.Status);
    }

    [Fact]
    public void ToNameValidationResult_WhenDataIsAnObject_LeavesTheNameEmpty()
    {
        var result = Envelope("""{"status":1,"code":"AVD01","data":{"name":"X"}}""").ToNameValidationResult();

        Assert.Equal(string.Empty, result.AccountHolderName);
    }

    [Fact]
    public void ToInternalTransferResult_WhenDataIsNotAString_LeavesTheIdEmpty()
    {
        var result = Envelope("""{"status":1,"code":"TR099","data":null}""").ToInternalTransferResult();

        Assert.Equal(string.Empty, result.TransferId);
    }

    [Fact]
    public void ToTransactionResult_ReadsFeesOnlyWhenTheyAreSent()
    {
        var withFees = Envelope("""{"status":1,"code":"SS01","data":{"txstatus":1,"fee":"0.5","networkfee":"0"}}""").ToTransactionResult();
        var withoutFees = Envelope("""{"status":1,"code":"SS01","data":{"txstatus":1}}""").ToTransactionResult();

        Assert.Equal(0.5m, withFees.Transaction.Fee);
        Assert.Equal(0m, withFees.Transaction.NetworkFee);
        Assert.Null(withoutFees.Transaction.Fee);
        Assert.Null(withoutFees.Transaction.NetworkFee);
    }

    [Theory]
    [InlineData("\"not a date\"")]
    [InlineData("null")]
    [InlineData("12345")]
    public void ToTransactionResult_WithAnUnreadableTimestamp_UsesTheDefaultInsteadOfThrowing(string ts)
    {
        var result = Envelope("""{"status":1,"code":"SS01","data":{"ts":""" + ts + "}}").ToTransactionResult();

        Assert.Equal(default, result.Transaction.Timestamp);
    }
}
