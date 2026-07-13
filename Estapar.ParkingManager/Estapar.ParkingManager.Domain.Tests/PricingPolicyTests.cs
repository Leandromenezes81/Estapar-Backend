using Estapar.ParkingManager.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Estapar.ParkingManager.Domain.Tests;

public class PricingPolicyTests
{
    [Theory]
    [InlineData(0.00, 0.90)]
    [InlineData(0.10, 0.90)]
    [InlineData(0.24, 0.90)]
    public void CalculateFactor_BelowTwentyFivePercent_AppliesDiscount(decimal occupancyRatio, decimal expectedFactor)
    {
        PricingPolicy.CalculateFactor(occupancyRatio).Should().Be(expectedFactor);
    }

    [Theory]
    [InlineData(0.25, 1.00)]
    [InlineData(0.40, 1.00)]
    [InlineData(0.50, 1.00)]
    public void CalculateFactor_UpToFiftyPercent_AppliesNormalPrice(decimal occupancyRatio, decimal expectedFactor)
    {
        PricingPolicy.CalculateFactor(occupancyRatio).Should().Be(expectedFactor);
    }

    [Theory]
    [InlineData(0.51, 1.10)]
    [InlineData(0.60, 1.10)]
    [InlineData(0.75, 1.10)]
    public void CalculateFactor_UpToSeventyFivePercent_AppliesTenPercentSurcharge(decimal occupancyRatio, decimal expectedFactor)
    {
        PricingPolicy.CalculateFactor(occupancyRatio).Should().Be(expectedFactor);
    }

    [Theory]
    [InlineData(0.76, 1.25)]
    [InlineData(0.90, 1.25)]
    [InlineData(1.00, 1.25)]
    public void CalculateFactor_UpToFullCapacity_AppliesTwentyFivePercentSurcharge(decimal occupancyRatio, decimal expectedFactor)
    {
        PricingPolicy.CalculateFactor(occupancyRatio).Should().Be(expectedFactor);
    }

    [Fact]
    public void CalculateFactor_NegativeRatio_Throws()
    {
        var act = () => PricingPolicy.CalculateFactor(-0.01m);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
