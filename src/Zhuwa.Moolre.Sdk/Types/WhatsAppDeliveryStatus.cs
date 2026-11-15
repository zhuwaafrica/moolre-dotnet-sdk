namespace Zhuwa.Moolre;

/// <summary>An extensible string value. Moolre can add new values, so this is not an enum: unknown values are kept exactly as received.</summary>
public readonly struct WhatsAppDeliveryStatus : IEquatable<WhatsAppDeliveryStatus>
{
    /// <summary>Creates a value from its text.</summary>
    /// <param name="value">The exact text Moolre sends or expects.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null, empty or white space.</exception>
    public WhatsAppDeliveryStatus(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>The exact text of the value (empty for a default instance).</summary>
    public string Value { get => field ?? string.Empty; }

    /// <summary>Creates a value from its text.</summary>
    /// <param name="value">The exact text Moolre sends or expects.</param>
    /// <returns>The value.</returns>
    public static WhatsAppDeliveryStatus FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(WhatsAppDeliveryStatus other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WhatsAppDeliveryStatus other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>Whether two values have the same text (case-sensitive).</summary>
    public static bool operator ==(WhatsAppDeliveryStatus left, WhatsAppDeliveryStatus right) => left.Equals(right);

    /// <summary>Whether two values have different text.</summary>
    public static bool operator !=(WhatsAppDeliveryStatus left, WhatsAppDeliveryStatus right) => !left.Equals(right);

    /// <summary>Converts text to a value.</summary>
    public static implicit operator WhatsAppDeliveryStatus(string value) => new(value);

    /// <summary>Read (<c>read</c>).</summary>
    public static WhatsAppDeliveryStatus Read { get; } = new("read");

    /// <summary>Accepted (<c>accepted</c>).</summary>
    public static WhatsAppDeliveryStatus Accepted { get; } = new("accepted");
}
