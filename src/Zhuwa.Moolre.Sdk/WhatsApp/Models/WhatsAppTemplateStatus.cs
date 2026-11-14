namespace Zhuwa.Moolre.WhatsApp;

/// <summary>An extensible string value. Moolre can add new values, so this is not an enum: unknown values are kept exactly as received.</summary>
public readonly struct WhatsAppTemplateStatus : IEquatable<WhatsAppTemplateStatus>
{
    /// <summary>Creates a value from its text.</summary>
    /// <param name="value">The exact text Moolre sends or expects.</param>
    /// <exception cref="ArgumentException"><paramref name="value"/> is null, empty or white space.</exception>
    public WhatsAppTemplateStatus(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value;
    }

    /// <summary>The exact text of the value (empty for a default instance).</summary>
    public string Value { get => field ?? string.Empty; }

    /// <summary>Creates a value from its text.</summary>
    /// <param name="value">The exact text Moolre sends or expects.</param>
    /// <returns>The value.</returns>
    public static WhatsAppTemplateStatus FromString(string value) => new(value);

    /// <inheritdoc/>
    public bool Equals(WhatsAppTemplateStatus other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is WhatsAppTemplateStatus other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <inheritdoc/>
    public override string ToString() => Value;

    /// <summary>Whether two values have the same text (case-sensitive).</summary>
    public static bool operator ==(WhatsAppTemplateStatus left, WhatsAppTemplateStatus right) => left.Equals(right);

    /// <summary>Whether two values have different text.</summary>
    public static bool operator !=(WhatsAppTemplateStatus left, WhatsAppTemplateStatus right) => !left.Equals(right);

    /// <summary>Converts text to a value.</summary>
    public static implicit operator WhatsAppTemplateStatus(string value) => new(value);

    /// <summary>Approved (<c>APPROVED</c>).</summary>
    public static WhatsAppTemplateStatus Approved { get; } = new("APPROVED");

    /// <summary>Rejected (<c>REJECTED</c>).</summary>
    public static WhatsAppTemplateStatus Rejected { get; } = new("REJECTED");

    /// <summary>Pending (<c>PENDING</c>).</summary>
    public static WhatsAppTemplateStatus Pending { get; } = new("PENDING");
}
