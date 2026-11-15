using System.Globalization;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// The codes Moolre expects for the SDK's enums. Clients call these to check a request before anything is sent (an unset or unknown
/// value throws with the property name); the JSON converters call the same methods to write the value, so both always agree.
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
    /// An amount as Moolre's text form, for example <c>"10.5"</c>. Always the invariant culture, so a server or user culture that writes
    /// a decimal comma can never change the amount sent.
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

    /// <summary>Moolre's transfer channel code. Numbers sent as text; the named values used by sales are not accepted here.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The channel is <see cref="TransferChannel.None"/> or not a known value.</exception>
    internal static string ToWire(this TransferChannel channel, string paramName) => channel switch
    {
        TransferChannel.Mtn => "1",
        TransferChannel.Bank => "2",
        TransferChannel.Telecel => "6",
        TransferChannel.At => "7",
        _ => throw new ArgumentOutOfRangeException(paramName, channel, "Choose a transfer channel: Mtn, Telecel, At or Bank."),
    };

    /// <summary>Moolre's collection channel code. MTN is 13 here (not 1 as for transfers).</summary>
    /// <exception cref="ArgumentOutOfRangeException">The channel is <see cref="PaymentChannel.None"/> or not a known value.</exception>
    internal static string ToWire(this PaymentChannel channel, string paramName) => channel switch
    {
        PaymentChannel.Mtn => "13",
        PaymentChannel.Telecel => "6",
        PaymentChannel.At => "7",
        _ => throw new ArgumentOutOfRangeException(paramName, channel, "Choose a payment channel: Mtn, Telecel or At."),
    };

    /// <summary>Moolre's <c>txstatus</c> value: <c>0</c> pending, <c>1</c> successful, <c>2</c> failed.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The status is not a known value.</exception>
    internal static int ToWire(this TransactionStatus status, string paramName) => status switch
    {
        TransactionStatus.Pending => 0,
        TransactionStatus.Successful => 1,
        TransactionStatus.Failed => 2,
        _ => throw new ArgumentOutOfRangeException(paramName, status, "Choose Pending, Successful or Failed."),
    };
}
