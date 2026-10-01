using PrimaFoods.Abate.Application.Models;

namespace PrimaFoods.Abate.UnitTests.Models;

public class PaymentSettingsTests
{
    [Fact]
    public void Defaults_AreValidAndMatchBusinessRules()
    {
        var settings = new PaymentSettings();

        Assert.True(settings.IsValid(out var error), error);
        Assert.Equal(100m, settings.MaleArrobaPrice);
        Assert.Equal(50m, settings.FemaleArrobaPrice);
        Assert.Equal(10m, settings.PremiumPercentage);
        Assert.Equal(10m, settings.DiscountPercentage);
    }

    [Theory]
    [InlineData(0, 50, 10, 10)]
    [InlineData(100, -1, 10, 10)]
    [InlineData(100, 50, -0.01, 10)]
    [InlineData(100, 50, 10, 100.01)]
    public void IsValid_RejectsOutOfRangeValues(double male, double female, double premium, double discount)
    {
        var settings = new PaymentSettings
        {
            MaleArrobaPrice = (decimal)male,
            FemaleArrobaPrice = (decimal)female,
            PremiumPercentage = (decimal)premium,
            DiscountPercentage = (decimal)discount
        };

        Assert.False(settings.IsValid(out var error));
        Assert.NotEmpty(error);
    }

    [Fact]
    public void IsValid_AcceptsZeroAndHundredPercent()
        => Assert.True(new PaymentSettings { PremiumPercentage = 0m, DiscountPercentage = 100m }.IsValid(out _));
}
