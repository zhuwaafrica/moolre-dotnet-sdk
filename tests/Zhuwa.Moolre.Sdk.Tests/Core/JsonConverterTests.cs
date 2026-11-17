using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using Zhuwa.Moolre.Core;
using Zhuwa.Moolre.Transfers;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests every JSON converter on its own, with the odd shapes Moolre sends (numbers as text, text as numbers, objects where a value was
/// expected) and the exact text each writes. A converter must never throw on an odd value and must leave the reader past it.
/// </summary>
public sealed class JsonConverterTests
{
    private static readonly JsonSerializerOptions Options = MoolreJsonContext.Default.Options;

    private static T? Read<T>(JsonConverter<T> converter, string json)
    {
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        var value = converter.Read(ref reader, typeof(T), Options);

        // The converter must have consumed the whole value, including a skipped object or array.
        Assert.False(reader.Read(), $"The converter left part of {json} unread.");
        return value;
    }

    private static string Write<T>(JsonConverter<T> converter, T value)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            converter.Write(writer, value, Options);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    [Theory]
    [InlineData("\"text\"", "text")]
    [InlineData("12.50", "12.50")]
    [InlineData("true", "true")]
    [InlineData("false", "false")]
    [InlineData("{\"a\":{\"b\":1}}", null)]
    [InlineData("[1,[2,3]]", null)]
    public void MoolreString_ReadsTextNumbersAndBooleansAndSkipsObjects(string json, string? expected) =>
        Assert.Equal(expected, Read(new MoolreStringConverter(), json));

    [Fact]
    public void MoolreString_TrimsWhenWriting() => Assert.Equal("\"value\"", Write(new MoolreStringConverter(), "  value  "));

    [Theory]
    [InlineData("10.67", 10.67)]
    [InlineData("\"1.01\"", 1.01)]
    [InlineData("\"1,500.00\"", 1500)]
    [InlineData("\"abc\"", 0)]
    [InlineData("null", 0)]
    [InlineData("true", 0)]
    [InlineData("{\"amount\":5}", 0)]
    public void LenientDecimal_ReadsNumbersOrTextAndAnythingElseAsZero(string json, double expected) =>
        Assert.Equal((decimal)expected, Read(new LenientDecimalConverter(), json));

    [Theory]
    [InlineData(10.5, "\"10.5\"")]
    [InlineData(1, "\"1\"")]
    [InlineData(0.05, "\"0.05\"")]
    [InlineData(1234567.89, "\"1234567.89\"")]
    public void DecimalAsText_WritesMoolresTextForm(double amount, string expected) =>
        Assert.Equal(expected, Write(new DecimalAsTextConverter(), (decimal)amount));

    [Theory]
    [InlineData("5", 5)]
    [InlineData("\"5\"", 5)]
    [InlineData("5.5", 0)]
    [InlineData("\"x\"", 0)]
    [InlineData("[5]", 0)]
    public void LenientInt32_ReadsWholeNumbersOrText(string json, int expected) => Assert.Equal(expected, Read(new LenientInt32Converter(), json));

    [Fact]
    public void Int32AsText_WritesText() => Assert.Equal("\"50\"", Write(new Int32AsTextConverter(), 50));

    [Theory]
    [InlineData("true", true)]
    [InlineData("1", true)]
    [InlineData("\"1\"", true)]
    [InlineData("\"true\"", true)]
    [InlineData("\"TRUE\"", true)]
    [InlineData("false", false)]
    [InlineData("0", false)]
    [InlineData("2", false)]
    [InlineData("\"0\"", false)]
    [InlineData("\"yes\"", false)]
    [InlineData("{}", false)]
    public void LenientBoolean_OnlyTreatsOneOrTrueAsTrue(string json, bool expected) => Assert.Equal(expected, Read(new LenientBooleanConverter(), json));

    [Theory]
    [InlineData(true, "\"1\"")]
    [InlineData(false, "\"0\"")]
    public void BooleanAsText_WritesOneOrZeroAsText(bool value, string expected) => Assert.Equal(expected, Write(new BooleanAsTextConverter(), value));

    [Fact]
    public void EmptyWhenNull_WritesEmptyTextForNullAndReadsEmptyAsNull()
    {
        Assert.Equal("\"\"", Write(new EmptyWhenNullStringConverter(), null));
        Assert.Equal("\"GHS\"", Write(new EmptyWhenNullStringConverter(), " GHS "));
        Assert.Null(Read(new EmptyWhenNullStringConverter(), "\"\""));
        Assert.Null(Read(new EmptyWhenNullStringConverter(), "\"   \""));
    }

    [Fact]
    public void Timestamp_ReadsGmtAndWritesUtc()
    {
        Assert.Equal(new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero), Read(new MoolreTimestampConverter(), "\"2026-01-02 03:04:05\""));
        Assert.Equal("\"2026-01-02 01:04:05\"", Write(new MoolreTimestampConverter(), new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(2))));
    }

    [Theory]
    [InlineData("\"2026-13-40 99:99:99\"")]
    [InlineData("\"2026-01-02T03:04:05Z\"")]
    [InlineData("12345")]
    [InlineData("null")]
    [InlineData("{}")]
    public void Timestamp_ReadsAnythingElseAsTheDefault(string json) => Assert.Equal(default, Read(new MoolreTimestampConverter(), json));

    [Theory]
    [InlineData("\"https://example.com/callback\"", "https://example.com/callback")]
    [InlineData("\"\"", null)]
    [InlineData("\"/relative/path\"", null)]
    [InlineData("\"file:///etc/passwd\"", null)]
    [InlineData("\"ftp://example.com/file\"", null)]
    [InlineData("\"http://example.com/callback\"", "http://example.com/callback")]
    [InlineData("5", null)]
    public void AbsoluteUri_ReadsOnlyAbsoluteUrls(string json, string? expected) =>
        Assert.Equal(expected is null ? null : new Uri(expected), Read(new AbsoluteUriConverter(), json));

    [Fact]
    public void WholeMinutes_WritesAndReadsMinutes()
    {
        Assert.Equal("30", Write(new WholeMinutesConverter(), TimeSpan.FromMinutes(30)));
        Assert.Equal(TimeSpan.FromMinutes(15), Read(new WholeMinutesConverter(), "\"15\""));
    }

    [Theory]
    [InlineData("\"GHS\"", Currency.Ghs)]
    [InlineData("\"NGN\"", Currency.Ngn)]
    [InlineData("\"USD\"", Currency.None)]
    [InlineData("1", Currency.None)]
    public void Currency_ReadsMoolresCodes(string json, Currency expected) => Assert.Equal(expected, Read(new CurrencyConverter(), json));

    [Fact]
    public void Currency_WritingNone_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => Write(new CurrencyConverter(), Currency.None));

    [Theory]
    [InlineData("\"1\"", TransferChannel.Mtn)]
    [InlineData("2", TransferChannel.Bank)]
    [InlineData("\"6\"", TransferChannel.Telecel)]
    [InlineData("7", TransferChannel.At)]
    [InlineData("\"9\"", TransferChannel.None)]
    public void TransferChannel_ReadsMoolresCodes(string json, TransferChannel expected) =>
        Assert.Equal(expected, Read(new TransferChannelConverter(), json));

    [Theory]
    [InlineData("\"13\"", PaymentChannel.Mtn)]
    [InlineData("6", PaymentChannel.Telecel)]
    [InlineData("\"7\"", PaymentChannel.At)]
    [InlineData("\"1\"", PaymentChannel.None)]
    public void PaymentChannel_ReadsMoolresCodesWhereMtnIs13(string json, PaymentChannel expected) =>
        Assert.Equal(expected, Read(new PaymentChannelConverter(), json));

    [Theory]
    [InlineData("\"MTN\"", SalesNetwork.Mtn)]
    [InlineData("\"mtn\"", SalesNetwork.Mtn)]
    [InlineData("\"TELECEL\"", SalesNetwork.Telecel)]
    [InlineData("6", SalesNetwork.Telecel)]
    [InlineData("\"7\"", SalesNetwork.At)]
    [InlineData("\"GLO\"", SalesNetwork.None)]
    public void SalesNetwork_ReadsNamesAndLegacyCodes(string json, SalesNetwork expected) =>
        Assert.Equal(expected, Read(new SalesNetworkConverter(), json));

    [Theory]
    [InlineData("1", SaleIdKind.ExternalReference)]
    [InlineData("\"2\"", SaleIdKind.TransactionId)]
    [InlineData("3", SaleIdKind.SaleReference)]
    [InlineData("4", SaleIdKind.None)]
    public void SaleIdKind_ReadsMoolresCodes(string json, SaleIdKind expected) => Assert.Equal(expected, Read(new SaleIdKindConverter(), json));

    [Theory]
    [InlineData("1", TransactionStatus.Successful)]
    [InlineData("\"2\"", TransactionStatus.Failed)]
    [InlineData("0", TransactionStatus.Pending)]
    [InlineData("7", TransactionStatus.Pending)]
    [InlineData("null", TransactionStatus.Pending)]
    [InlineData("\"done\"", TransactionStatus.Pending)]
    public void TransactionStatus_NeverReadsAnUnknownValueAsSuccessful(string json, TransactionStatus expected) =>
        Assert.Equal(expected, Read(new TransactionStatusConverter(), json));

    [Fact]
    public void TransactionStatus_WritesNumbers() => Assert.Equal("2", Write(new TransactionStatusConverter(), TransactionStatus.Failed));

    [Theory]
    [InlineData("1", TransactionIdKind.ExternalReference)]
    [InlineData("\"2\"", TransactionIdKind.MoolreId)]
    [InlineData("3", TransactionIdKind.None)]
    public void TransactionIdKind_ReadsMoolresCodes(string json, TransactionIdKind expected) =>
        Assert.Equal(expected, Read(new TransactionIdKindConverter(), json));

    [Theory]
    [InlineData("20.73", 20.73)]
    [InlineData("\"20.73\"", 20.73)]
    [InlineData("true", 0)]
    public void LenientDouble_ReadsNumbersOrText(string json, double expected) => Assert.Equal(expected, Read(new LenientDoubleConverter(), json));

    [Theory]
    [InlineData("\"one\"", new[] { "one" })]
    [InlineData("[\"one\",1,\"two\",null,{}]", new[] { "one", "two" })]
    [InlineData("null", new string[0])]
    [InlineData("\"\"", new string[0])]
    [InlineData("5", new string[0])]
    public void Messages_TurnsEveryShapeIntoAList(string json, string[] expected) => Assert.Equal(expected, Read(new MessagesConverter(), json));

    [Fact]
    public void OneOrMany_ForTextOrAnEmptyObject_GivesAnEmptyList()
    {
        Assert.Empty(Read(new OneOrManyConverter<TransactionWire>(), "\"none\"")!);
        Assert.Empty(Read(new OneOrManyConverter<TransactionWire>(), "{}")!);
        Assert.Single(Read(new OneOrManyConverter<TransactionWire>(), "{\"txstatus\":1}")!);
    }

    [Fact]
    public void LenientObject_ForAnythingButAnObject_GivesNull()
    {
        Assert.Null(Read(new LenientObjectConverter<PaymentAccountWire>(), "\"none\""));
        Assert.Null(Read(new LenientObjectConverter<PaymentAccountWire>(), "[1,2]"));
        Assert.NotNull(Read(new LenientObjectConverter<PaymentAccountWire>(), "{\"accountno\":\"1\"}"));
    }

    [Fact]
    public void ObjectList_ForSomethingThatIsNotAList_GivesAnEmptyList() =>
        Assert.Empty(Read(new ObjectListConverter<BankWire>(), "{\"name\":\"A\"}")!);

    [Fact]
    public void TextConverters_ReadBackWhatTheyWrite()
    {
        Assert.True(Read(new BooleanAsTextConverter(), "\"1\""));
        Assert.Equal(10.5m, Read(new DecimalAsTextConverter(), "\"10.5\""));
        Assert.Equal(50, Read(new Int32AsTextConverter(), "\"50\""));
        Assert.Equal("7", Write(new LenientInt32Converter(), 7));
        Assert.Equal("1.5", Write(new LenientDecimalConverter(), 1.5m));
        Assert.Equal("true", Write(new LenientBooleanConverter(), true));
        Assert.Equal("2.5", Write(new LenientDoubleConverter(), 2.5));
        Assert.Equal("\"https://example.com/\"", Write(new AbsoluteUriConverter(), new Uri("https://example.com")));
    }
}
