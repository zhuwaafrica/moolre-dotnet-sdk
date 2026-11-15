using System.Globalization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Converts shared SDK values to the text Moolre expects in a request. Unknown values throw instead of sending something Moolre
/// would misread. Area-specific values (channels, networks) are converted in their own area.
/// </summary>
internal static class WireValues
{
    /// <summary>The currency code, <c>GHS</c> or <c>NGN</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The currency is <see cref="Currency.None"/> or not a known value.</exception>
    internal static string ToWire(this Currency currency, string paramName) => currency switch
    {
        Currency.Ghs => "GHS",
        Currency.Ngn => "NGN",
        _ => throw new ArgumentOutOfRangeException(paramName, currency, "Choose a currency: Ghs or Ngn."),
    };

    /// <summary>
    /// An amount as Moolre's string form, for example <c>"10.5"</c>. Always the invariant culture, so a server or user culture that
    /// writes a decimal comma can never change the amount sent.
    /// </summary>
    internal static string ToWire(this decimal amount) => amount.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>The <c>idtype</c> value for a status lookup: <c>1</c> external reference, <c>2</c> Moolre ID.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The kind is <see cref="TransactionIdKind.None"/> or not a known value.</exception>
    internal static int ToWire(this TransactionIdKind kind, string paramName) => kind switch
    {
        TransactionIdKind.ExternalReference => 1,
        TransactionIdKind.MoolreId => 2,
        _ => throw new ArgumentOutOfRangeException(paramName, kind, "Choose ExternalReference or MoolreId."),
    };
}
