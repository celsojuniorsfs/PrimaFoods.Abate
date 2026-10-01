using PrimaFoods.Abate.Application.Common;

namespace PrimaFoods.Abate.UnitTests.Common;

public class CalculationsTests
{
    [Fact]
    public void Percentage_ReturnsZero_WhenTotalIsZero()
        => Assert.Equal(0m, Calculations.Percentage(5, 0));

    [Theory]
    [InlineData(1, 3, 33.33)]
    [InlineData(2, 3, 66.67)]
    [InlineData(1, 8, 12.5)]
    [InlineData(1, 200, 0.5)]
    [InlineData(10, 10, 100)]
    public void Percentage_RoundsToTwoDecimals(int part, int total, double expected)
        => Assert.Equal((decimal)expected, Calculations.Percentage(part, total));

    [Fact]
    public void Divide_ReturnsZero_WhenDivisorIsZero()
        => Assert.Equal(0m, Calculations.Divide(10m, 0m));

    [Theory]
    [InlineData(10, 3, 3.33)]
    [InlineData(1, 8, 0.13)]
    [InlineData(100, 4, 25)]
    public void Divide_RoundsAwayFromZeroToTwoDecimals(double dividend, double divisor, double expected)
        => Assert.Equal((decimal)expected, Calculations.Divide((decimal)dividend, (decimal)divisor));
}
