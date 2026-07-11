using Estapar.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Estapar.Domain.Tests;

public class FeeCalculatorTests
{
    private static readonly DateTime EntryTime = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    public void Calculate_UpToThirtyMinutes_IsFree(int minutesParked)
    {
        var exitTime = EntryTime.AddMinutes(minutesParked);

        var fee = FeeCalculator.Calculate(EntryTime, exitTime, basePrice: 10m, priceFactor: 1.0m);

        fee.Amount.Should().Be(0m);
    }

    [Fact]
    public void Calculate_JustOverThirtyMinutes_ChargesOneFullHour()
    {
        var exitTime = EntryTime.AddMinutes(31);

        var fee = FeeCalculator.Calculate(EntryTime, exitTime, basePrice: 10m, priceFactor: 1.0m);

        fee.Amount.Should().Be(10m);
    }

    [Fact]
    public void Calculate_ExactlyOneHour_ChargesOneFullHour()
    {
        var exitTime = EntryTime.AddMinutes(60);

        var fee = FeeCalculator.Calculate(EntryTime, exitTime, basePrice: 10m, priceFactor: 1.0m);

        fee.Amount.Should().Be(10m);
    }

    [Fact]
    public void Calculate_JustOverOneHour_RoundsUpToTwoHours()
    {
        var exitTime = EntryTime.AddMinutes(61);

        var fee = FeeCalculator.Calculate(EntryTime, exitTime, basePrice: 10m, priceFactor: 1.0m);

        fee.Amount.Should().Be(20m);
    }

    [Fact]
    public void Calculate_AppliesLockedPriceFactor()
    {
        var exitTime = EntryTime.AddMinutes(45);

        var fee = FeeCalculator.Calculate(EntryTime, exitTime, basePrice: 10m, priceFactor: 0.90m);

        fee.Amount.Should().Be(9m);
    }

    [Fact]
    public void Calculate_ReturnsAmountInBrl()
    {
        var exitTime = EntryTime.AddMinutes(45);

        var fee = FeeCalculator.Calculate(EntryTime, exitTime, basePrice: 10m, priceFactor: 1.0m);

        fee.Currency.Should().Be("BRL");
    }

    [Fact]
    public void Calculate_ExitBeforeEntry_Throws()
    {
        var exitTime = EntryTime.AddMinutes(-1);

        var act = () => FeeCalculator.Calculate(EntryTime, exitTime, basePrice: 10m, priceFactor: 1.0m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
