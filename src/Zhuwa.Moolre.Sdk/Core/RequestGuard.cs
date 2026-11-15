using System.Runtime.CompilerServices;

namespace Zhuwa.Moolre.Core;

/// <summary>
/// Checks on request values that the base library does not provide. Every check runs before anything is sent, so a bad request
/// never reaches Moolre. The parameter name comes from the caller's expression (for example <c>request.Amount</c>).
/// </summary>
internal static class RequestGuard
{
    /// <summary>The most decimal places Moolre amounts carry (pesewas / kobo).</summary>
    private const int MaxDecimalPlaces = 2;

    /// <summary>Throws unless the amount is above zero with at most two decimal places.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The amount is zero, negative or has more than two decimal places.</exception>
    internal static void ThrowIfInvalidAmount(decimal amount, [CallerArgumentExpression(nameof(amount))] string? paramName = null)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(paramName, amount, "The amount must be greater than zero.");
        }

        // Rejected instead of rounded: silently changing a money amount is worse than refusing it.
        if (decimal.Round(amount, MaxDecimalPlaces) != amount)
        {
            throw new ArgumentOutOfRangeException(paramName, amount, "The amount must have at most two decimal places.");
        }
    }

    /// <summary>Throws when a value that is only required in some cases (for example the bank code for a bank transfer) is blank.</summary>
    /// <exception cref="ArgumentException">The value is null, empty or white space.</exception>
    internal static void ThrowIfBlank(string? value, string reason, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(reason, paramName);
        }
    }

    /// <summary>
    /// Throws unless a duration is a whole number of minutes, at least one (Moolre takes payment link expiry in whole minutes). A part
    /// minute is refused, not rounded, so the link never lives longer or shorter than the caller asked.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The duration is under one minute or not whole minutes.</exception>
    internal static void ThrowIfNotWholeMinutes(TimeSpan? duration, [CallerArgumentExpression(nameof(duration))] string? paramName = null)
    {
        if (duration is TimeSpan value
            && (value < TimeSpan.FromMinutes(1) || value.Ticks % TimeSpan.TicksPerMinute != 0 || value.TotalMinutes > int.MaxValue))
        {
            throw new ArgumentOutOfRangeException(paramName, value, "The expiry must be a whole number of minutes, at least one.");
        }
    }
}
