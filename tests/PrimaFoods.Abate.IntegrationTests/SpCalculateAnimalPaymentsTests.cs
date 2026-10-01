using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.IntegrationTests;

/// <summary>
/// Cobre a regra de pagamento, que vive em database/04_spCalculateAnimalPayments.sql:
/// 1 @ = 15 kg, macho R$ 100/@, fêmea R$ 50/@, ágio de 10% com 0 dentes, deságio de 10% com 6+ dentes.
/// </summary>
[Collection(SqlServerCollection.Name)]
public sealed class SpCalculateAnimalPaymentsTests(SqlServerFixture db) : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await db.ResetAsync();
        await db.AddOrderAsync(1, "João Silva");
        await db.AddOrderAsync(2, "Pedro Silva");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<AnimalPaymentView> CalculateSingleAsync(string sex, int teeth, decimal weight)
    {
        await db.AddAnimalAsync(1, 1, sex, teeth, weight);

        var processed = (await db.Calculator.CalculateAsync()).ProcessedAnimals;
        var payments = await db.Reader.GetAllAsync();

        Assert.Equal(1, processed);
        return Assert.Single(payments);
    }

    [Fact]
    public async Task Male_WithTwoTeeth_PaysBaseAmountWithoutAdjustment()
    {
        var payment = await CalculateSingleAsync("M", teeth: 2, weight: 300m);

        Assert.Equal(Sex.Male, payment.Sex);
        Assert.Equal(20m, payment.Arrobas);
        Assert.Equal(100m, payment.BaseArrobaPrice);
        Assert.Equal(2000m, payment.BaseAmount);
        Assert.Equal(0m, payment.PremiumAmount);
        Assert.Equal(0m, payment.DiscountAmount);
        Assert.Equal(2000m, payment.AmountToPay);
        Assert.Equal(AdjustmentType.None, payment.AdjustmentType);
    }

    [Fact]
    public async Task Female_UsesFemaleArrobaPrice()
    {
        var payment = await CalculateSingleAsync("F", teeth: 2, weight: 150m);

        Assert.Equal(Sex.Female, payment.Sex);
        Assert.Equal(10m, payment.Arrobas);
        Assert.Equal(50m, payment.BaseArrobaPrice);
        Assert.Equal(500m, payment.BaseAmount);
        Assert.Equal(500m, payment.AmountToPay);
    }

    [Fact]
    public async Task ZeroTeeth_AddsTenPercentPremium()
    {
        var payment = await CalculateSingleAsync("M", teeth: 0, weight: 300m);

        Assert.Equal(AdjustmentType.Premium, payment.AdjustmentType);
        Assert.Equal(200m, payment.PremiumAmount);
        Assert.Equal(0m, payment.DiscountAmount);
        Assert.Equal(2200m, payment.AmountToPay);
        Assert.Contains("Ágio", payment.AdjustmentReason);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public async Task SixOrMoreTeeth_SubtractsTenPercentDiscount(int teeth)
    {
        var payment = await CalculateSingleAsync("M", teeth, weight: 300m);

        Assert.Equal(AdjustmentType.Discount, payment.AdjustmentType);
        Assert.Equal(0m, payment.PremiumAmount);
        Assert.Equal(200m, payment.DiscountAmount);
        Assert.Equal(1800m, payment.AmountToPay);
        Assert.Contains("Deságio", payment.AdjustmentReason);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public async Task OneToFiveTeeth_HaveNoAdjustment(int teeth)
    {
        var payment = await CalculateSingleAsync("M", teeth, weight: 300m);

        Assert.Equal(AdjustmentType.None, payment.AdjustmentType);
        Assert.Equal(payment.BaseAmount, payment.AmountToPay);
    }

    [Fact]
    public async Task FractionalWeight_RoundsAmountsToTwoDecimals()
    {
        // 294,5 kg / 15 = 19,6333 @ ; 294,5 * 100 / 15 = 1963,33
        var payment = await CalculateSingleAsync("M", teeth: 2, weight: 294.5m);

        Assert.Equal(19.6333m, payment.Arrobas);
        Assert.Equal(1963.33m, payment.BaseAmount);
        Assert.Equal(1963.33m, payment.AmountToPay);
    }

    [Fact]
    public async Task Premium_IsAppliedOnRoundedBaseAmount()
    {
        // base 1963,33 -> ágio 196,333 arredondado para 196,33 -> 2159,66
        var payment = await CalculateSingleAsync("M", teeth: 0, weight: 294.5m);

        Assert.Equal(196.33m, payment.PremiumAmount);
        Assert.Equal(2159.66m, payment.AmountToPay);
    }

    [Fact]
    public async Task InvalidAnimals_AreSkippedAndNotCounted()
    {
        await db.AddAnimalAsync(1, 1, "M", 2, 300m);
        await db.AddAnimalAsync(2, 1, "X", 2, 300m);    // sexo desconhecido
        await db.AddAnimalAsync(3, 1, null, 2, 300m);   // sem sexo
        await db.AddAnimalAsync(4, 1, "F", null, 300m); // sem dentes
        await db.AddAnimalAsync(5, 1, "F", 2, null);    // sem peso

        var processed = (await db.Calculator.CalculateAsync()).ProcessedAnimals;
        var payments = await db.Reader.GetAllAsync();

        Assert.Equal(1, processed);
        Assert.Equal(1, Assert.Single(payments).AnimalId);
    }

    [Fact]
    public async Task AnimalWithoutOrder_IsSkipped()
    {
        await db.AddAnimalAsync(1, 1, "M", 2, 300m);
        await db.AddAnimalAsync(2, null, "M", 2, 300m);

        var result = await db.Calculator.CalculateAsync();

        Assert.Equal(new CalculationResult(ProcessedAnimals: 1, SkippedAnimals: 1), result);
        Assert.Equal(1, Assert.Single(await db.Reader.GetAllAsync()).AnimalId);
    }

    [Fact]
    public async Task Recalculating_ReplacesPreviousResults_WithoutDuplicates()
    {
        await db.AddAnimalAsync(1, 1, "M", 2, 300m);
        await db.AddAnimalAsync(2, 2, "F", 0, 150m);

        await db.Calculator.CalculateAsync();
        var processed = (await db.Calculator.CalculateAsync()).ProcessedAnimals;
        var payments = await db.Reader.GetAllAsync();

        Assert.Equal(2, processed);
        Assert.Equal(2, payments.Count);
    }

    [Fact]
    public async Task Recalculating_ReflectsChangedAnimalData()
    {
        await db.AddAnimalAsync(1, 1, "M", 2, 300m);
        await db.Calculator.CalculateAsync();

        await db.ResetAsync();
        await db.AddOrderAsync(1, "João Silva");
        await db.Calculator.CalculateAsync();

        Assert.Empty(await db.Reader.GetAllAsync());
    }

    [Fact]
    public async Task Reader_ReturnsSupplierOrderedByOrderThenAnimal()
    {
        await db.AddAnimalAsync(3, 2, "M", 2, 300m);
        await db.AddAnimalAsync(2, 1, "F", 2, 150m);
        await db.AddAnimalAsync(1, 1, "M", 2, 300m);

        await db.Calculator.CalculateAsync();
        var payments = await db.Reader.GetAllAsync();

        Assert.Equal([1, 2, 3], payments.Select(p => p.AnimalId));
        Assert.Equal(["João Silva", "João Silva", "Pedro Silva"], payments.Select(p => p.Supplier));
    }

    [Fact]
    public async Task WithoutAnimals_ReturnsZeroProcessed()
    {
        var result = await db.Calculator.CalculateAsync();

        Assert.Equal(new CalculationResult(0, 0), result);
        Assert.Empty(await db.Reader.GetAllAsync());
    }

    [Fact]
    public async Task InvalidAnimals_AreReportedAsSkipped()
    {
        await db.AddAnimalAsync(1, 1, "M", 2, 300m);
        await db.AddAnimalAsync(2, 1, "X", 2, 300m);
        await db.AddAnimalAsync(3, 1, "F", null, 300m);
        await db.AddAnimalAsync(4, 1, "F", 2, null);

        var result = await db.Calculator.CalculateAsync();

        Assert.Equal(new CalculationResult(ProcessedAnimals: 1, SkippedAnimals: 3), result);
    }

    [Fact]
    public async Task ConfiguredSettings_ChangePricesAndPercentages()
    {
        await db.AddAnimalAsync(1, 1, "M", 0, 300m);
        await db.AddAnimalAsync(2, 1, "F", 6, 150m);

        var calculator = db.CreateCalculator(new PaymentSettings
        {
            MaleArrobaPrice = 200m,
            FemaleArrobaPrice = 80m,
            PremiumPercentage = 5m,
            DiscountPercentage = 25m
        });

        await calculator.CalculateAsync();
        var payments = await db.Reader.GetAllAsync();

        var male = payments.Single(p => p.AnimalId == 1);
        Assert.Equal(4000m, male.BaseAmount);      // 20 @ x 200
        Assert.Equal(200m, male.PremiumAmount);    // 5%
        Assert.Equal(4200m, male.AmountToPay);
        Assert.Contains("5", male.AdjustmentReason);

        var female = payments.Single(p => p.AnimalId == 2);
        Assert.Equal(800m, female.BaseAmount);     // 10 @ x 80
        Assert.Equal(200m, female.DiscountAmount); // 25%
        Assert.Equal(600m, female.AmountToPay);
        Assert.Contains("25", female.AdjustmentReason);
    }
}
