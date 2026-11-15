using System.Net;
using Xunit;
using Zhuwa.Moolre.Core;
using Zhuwa.Moolre.Reference;

namespace Zhuwa.Moolre;

/// <summary>Tests the reference data mapper on its own: a fixture goes in, a result comes out, with no HTTP.</summary>
public sealed class ReferenceDataMapperTests
{
    private static ResponseEnvelope Envelope(string fixture) =>
        ResponseEnvelope.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture)), HttpStatusCode.OK);

    [Fact]
    public void ToBankListResult_ForTheDocumentedExample_MapsEveryBankInOrder()
    {
        var result = Envelope("miscellaneous-data/response-200-success.json").ToBankListResult();

        Assert.Equal(["ABSA BANK GHANA LTD", "ACCESS BANK GHANA LTD", "ADB BANK GHANA LTD"], result.Banks.Select(b => b.Name));
        Assert.Equal(["030100", "030402", "030301"], result.Banks.Select(b => b.Code));
    }

    [Fact]
    public void ToBankListResult_WhenDataIsNull_ReturnsNoBanks()
    {
        var envelope = ResponseEnvelope.Parse("""{"status":1,"code":"SD01","data":null}"""u8, HttpStatusCode.OK);

        Assert.Empty(envelope.ToBankListResult().Banks);
    }
}
