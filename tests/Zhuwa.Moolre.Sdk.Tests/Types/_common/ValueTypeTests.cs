using Xunit;

namespace Zhuwa.Moolre;

/// <summary>
/// Tests the text-based value types (response codes, sender ID approval, WhatsApp statuses). They must compare exactly, keep values Moolre
/// adds later, and treat a default instance as empty rather than failing.
/// </summary>
public sealed class ValueTypeTests
{
    private static void AssertBehavesAsExactText<T>(Func<string, T> create, T knownValue, string knownText, Func<T, string> text)
        where T : struct, IEquatable<T>
    {
        var same = create(knownText);
        var other = create(knownText + "-other");
        var otherCase = create(string.Equals(knownText.ToLowerInvariant(), knownText, StringComparison.Ordinal)
            ? knownText.ToUpperInvariant()
            : knownText.ToLowerInvariant());

        Assert.Equal(knownValue, same);
        Assert.True(knownValue.Equals((object)same));
        Assert.Equal(knownValue.GetHashCode(), same.GetHashCode());
        Assert.NotEqual(knownValue, other);
        Assert.NotEqual(knownValue, otherCase);
        Assert.False(knownValue.Equals("not a value"));
        Assert.Equal(knownText, text(same));
        Assert.Equal(string.Empty, text(default));
        Assert.Throws<ArgumentException>(() => create(" "));
    }

    [Fact]
    public void MoolreResponseCode_BehavesAsExactText()
    {
        AssertBehavesAsExactText(v => new MoolreResponseCode(v), MoolreResponseCode.WalletFound, "SW01", v => v.Value);
        Assert.True(MoolreResponseCode.WalletFound == MoolreResponseCode.FromString("SW01"));
        Assert.True(MoolreResponseCode.WalletFound != (MoolreResponseCode)"SW02");
        Assert.Equal("SW01", MoolreResponseCode.WalletFound.ToString());
    }

    [Fact]
    public void SenderIdApproval_BehavesAsExactText()
    {
        AssertBehavesAsExactText(v => new SenderIdApproval(v), SenderIdApproval.Approved, "Approved", v => v.Value);
        Assert.True(SenderIdApproval.Pending == SenderIdApproval.FromString("Pending"));
        Assert.True(SenderIdApproval.Rejected != (SenderIdApproval)"Approved");
        Assert.Equal("Rejected", SenderIdApproval.Rejected.ToString());
    }

    [Fact]
    public void WhatsAppTemplateStatus_BehavesAsExactText()
    {
        AssertBehavesAsExactText(v => new WhatsAppTemplateStatus(v), WhatsAppTemplateStatus.Approved, "APPROVED", v => v.Value);
        Assert.True(WhatsAppTemplateStatus.Pending == WhatsAppTemplateStatus.FromString("PENDING"));
        Assert.True(WhatsAppTemplateStatus.Rejected != (WhatsAppTemplateStatus)"APPROVED");
        Assert.Equal("REJECTED", WhatsAppTemplateStatus.Rejected.ToString());
    }

    [Fact]
    public void WhatsAppDeliveryStatus_BehavesAsExactText()
    {
        AssertBehavesAsExactText(v => new WhatsAppDeliveryStatus(v), WhatsAppDeliveryStatus.Read, "read", v => v.Value);
        Assert.True(WhatsAppDeliveryStatus.Accepted == WhatsAppDeliveryStatus.FromString("accepted"));
        Assert.True(WhatsAppDeliveryStatus.Read != (WhatsAppDeliveryStatus)"accepted");
        Assert.Equal("accepted", WhatsAppDeliveryStatus.Accepted.ToString());
    }
}
