using System.Reflection;
using Xunit;

namespace Zhuwa.Moolre;

public sealed class MoolreResponseCodeTests
{
    [Fact]
    public void Equals_SameText_IsTrue()
    {
        var left = new MoolreResponseCode("TP14");
        var right = new MoolreResponseCode("TP14");

        Assert.True(left == right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Equals_SameTextDifferentCase_IsFalse()
    {
        var upper = new MoolreResponseCode("TP14");
        var lower = new MoolreResponseCode("tp14");

        Assert.True(upper != lower);
    }

    [Fact]
    public void ImplicitConversion_FromString_KeepsTheExactText()
    {
        MoolreResponseCode code = "SOMETHING_NEW";

        Assert.Equal("SOMETHING_NEW", code.Value);
        Assert.Equal("SOMETHING_NEW", code.ToString());
    }

    [Fact]
    public void ToString_ForADefaultInstance_IsEmpty()
    {
        var code = default(MoolreResponseCode);

        Assert.Equal(string.Empty, code.ToString());
        Assert.Equal(string.Empty, code.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankText_Throws(string value)
    {
        Assert.Throws<ArgumentException>(() => new MoolreResponseCode(value));
    }

    [Fact]
    public void Constructor_WithNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new MoolreResponseCode(null!));
    }

    [Fact]
    public void FromString_Always_MatchesTheConstructor()
    {
        Assert.Equal(new MoolreResponseCode("P01"), MoolreResponseCode.FromString("P01"));
    }

    [Theory]
    [InlineData("AuthenticationError", "AIN01")]
    [InlineData("OtpRequired", "TP14")]
    [InlineData("PaymentSucceeded", "P01")]
    [InlineData("PaymentFailed", "P02")]
    [InlineData("SaleInProgress", "ASL11")]
    [InlineData("SaleOutcomeUnknown", "ASL06")]
    [InlineData("WhatsAppSuccess", "WAS200")]
    [InlineData("SmsSent", "SMS01")]
    public void WellKnownCode_Always_HasTheDocumentedText(string member, string expected)
    {
        var property = typeof(MoolreResponseCode).GetProperty(member, BindingFlags.Public | BindingFlags.Static);

        var code = Assert.IsType<MoolreResponseCode>(property!.GetValue(null));

        Assert.Equal(expected, code.Value);
    }

    [Fact]
    public void WellKnownCodes_Always_HaveDistinctText()
    {
        var values = typeof(MoolreResponseCode)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(p => p.PropertyType == typeof(MoolreResponseCode))
            .Select(p => ((MoolreResponseCode)p.GetValue(null)!).Value)
            .ToList();

        var duplicates = values.GroupBy(v => v, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key);

        Assert.Empty(duplicates);
    }
}
