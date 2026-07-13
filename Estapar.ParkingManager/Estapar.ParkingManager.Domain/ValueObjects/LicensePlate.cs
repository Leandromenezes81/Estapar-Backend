namespace Estapar.ParkingManager.Domain.ValueObjects;

/// <summary>Objeto de valor que representa a placa de um veículo, sempre normalizada em maiúsculas.</summary>
public sealed class LicensePlate : IEquatable<LicensePlate>
{
    public string Value { get; }

    private LicensePlate(string value)
    {
        Value = value;
    }

    /// <summary>Cria uma placa a partir do texto informado, validando que não é vazio e normalizando para maiúsculas.</summary>
    public static LicensePlate Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("A placa não pode ser vazia.", nameof(value));

        return new LicensePlate(value.Trim().ToUpperInvariant());
    }

    /// <summary>Compara duas placas pelo valor normalizado.</summary>
    public bool Equals(LicensePlate? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) => Equals(obj as LicensePlate);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    public static implicit operator string(LicensePlate plate) => plate.Value;
}
