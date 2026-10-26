using System.Globalization;

namespace Plates.Engine;

/// <summary>A non-negative weight in kilograms. Used for every Target, Base weight, Increment and Record.</summary>
public readonly struct Weight : IEquatable<Weight>, IComparable<Weight>, IFormattable
{
    private const decimal KgPerPound = 0.45359237m;
    private const string KgSuffix = "kg";

    // Drops trailing zeros: 60.00 → "60", 42.750 → "42,75".
    private const string TrimmedNumber = "0.############################";

    private Weight(decimal kg)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(kg);
        Kg = kg;
    }

    public decimal Kg { get; }

    public static Weight FromKg(decimal kg) => new(kg);

    /// <summary>Parses "62,5 kg", "62,5kg" or "62,5"; the decimal separator follows the provider; the "kg" suffix is optional and case-insensitive.</summary>
    public static bool TryParse(string? text, IFormatProvider? provider, out Weight result)
    {
        result = default;
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        if (trimmed.EndsWith(KgSuffix, StringComparison.OrdinalIgnoreCase))
        {
            Range number = ..^KgSuffix.Length;
            trimmed = trimmed[number].TrimEnd();
        }

        // No thousands separators: under en-US "62,5" would otherwise parse as 625 kg.
        if (!decimal.TryParse(trimmed, NumberStyles.AllowDecimalPoint, provider, out var kg))
        {
            return false;
        }

        result = new Weight(kg);
        return true;
    }

    public bool Equals(Weight other) => Kg == other.Kg;

    public override bool Equals(object? obj) => obj is Weight other && Equals(other);

    // decimal hashes 60 and 60.0 alike, so equal weights share a hash.
    public override int GetHashCode() => Kg.GetHashCode();

    public static bool operator ==(Weight left, Weight right) => left.Equals(right);

    public static bool operator !=(Weight left, Weight right) => !left.Equals(right);

    public int CompareTo(Weight other) => Kg.CompareTo(other.Kg);

    public static bool operator <(Weight left, Weight right) => left.CompareTo(right) < 0;

    public static bool operator >(Weight left, Weight right) => left.CompareTo(right) > 0;

    public static bool operator <=(Weight left, Weight right) => left.CompareTo(right) <= 0;

    public static bool operator >=(Weight left, Weight right) => left.CompareTo(right) >= 0;

    public static Weight operator +(Weight left, Weight right) => new(left.Kg + right.Kg);

    public static Weight operator -(Weight left, Weight right) => new(left.Kg - right.Kg);

    public static Weight operator *(Weight weight, decimal factor) => new(weight.Kg * factor);

    public override string ToString() => ToString(null, null);

    /// <summary>
    /// Formats: "G" (also null or empty) → "62,5 kg"; "N" → "62,5"; "LB" → "137,8 lb".
    /// Case-insensitive. The decimal separator follows the provider; null means the current culture.
    /// </summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        formatProvider ??= CultureInfo.CurrentCulture;

        return (string.IsNullOrEmpty(format) ? "G" : format.ToUpperInvariant()) switch
        {
            "G" => Kg.ToString(TrimmedNumber, formatProvider) + " " + KgSuffix,
            "N" => Kg.ToString(TrimmedNumber, formatProvider),
            "LB" => Math.Round(Kg / KgPerPound, 1, MidpointRounding.AwayFromZero).ToString("0.0", formatProvider) + " lb",
            _ => throw new FormatException($"Unknown weight format '{format}'. Use G, N or LB."),
        };
    }
}
