namespace Estapar.Domain.ValueObjects;

public sealed class LicensePlate : IEquatable<LicensePlate>
{
    public string Value { get; }

    private LicensePlate(string value)
    {
        Value = value;
    }

    public static LicensePlate Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("License plate cannot be empty.", nameof(value));

        return new LicensePlate(value.Trim().ToUpperInvariant());
    }

    public bool Equals(LicensePlate? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as LicensePlate);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(LicensePlate plate) => plate.Value;
}
