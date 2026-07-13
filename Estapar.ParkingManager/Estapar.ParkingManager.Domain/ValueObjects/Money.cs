namespace Estapar.ParkingManager.Domain.ValueObjects;

/// <summary>Objeto de valor que representa uma quantia monetária com sua moeda.</summary>
public sealed class Money : IEquatable<Money>
{
    public const string DefaultCurrency = "BRL";

    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    /// <summary>Cria uma quantia monetária, validando que o valor não é negativo e que a moeda foi informada.</summary>
    public static Money Create(decimal amount, string currency = DefaultCurrency)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "O valor não pode ser negativo.");

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("A moeda não pode ser vazia.", nameof(currency));

        return new Money(amount, currency.ToUpperInvariant());
    }

    /// <summary>Cria uma quantia monetária zerada na moeda informada (ou na moeda padrão).</summary>
    public static Money Zero(string currency = DefaultCurrency) => Create(0m, currency);

    /// <summary>Compara duas quantias monetárias por valor e moeda.</summary>
    public bool Equals(Money? other) =>
        other is not null && Amount == other.Amount && Currency == other.Currency;

    public override bool Equals(object? obj) => Equals(obj as Money);

    public override int GetHashCode() => HashCode.Combine(Amount, Currency);

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
