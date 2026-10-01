using PrimaFoods.Abate.Application.Abstractions;
using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;
using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.UnitTests.UseCases;

public class GetPaymentDashboardUseCaseTests
{
    private sealed class FakeReader(params AnimalPaymentView[] payments) : IAnimalPaymentReader
    {
        public Task<IReadOnlyList<AnimalPaymentView>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AnimalPaymentView>>(payments);
    }

    private static Task<PaymentDashboardResult> Execute(params AnimalPaymentView[] payments)
        => new GetPaymentDashboardUseCase(new FakeReader(payments)).ExecuteAsync();

    [Fact]
    public async Task Execute_WithoutPayments_ReturnsEmptyDashboard()
    {
        var result = await Execute();

        Assert.False(result.HasData);
        Assert.Equal(0, result.TotalAnimals);
        Assert.Equal(0m, result.Males.Percentage);
        Assert.Equal(0m, result.Premiums.Percentage);
        Assert.Null(result.CalculatedAt);
        Assert.Empty(result.OrderSummaries);
        Assert.Empty(result.Animals);
    }

    [Fact]
    public async Task Execute_CountsSexAndAdjustmentsWithPercentages()
    {
        var result = await Execute(
            TestData.Payment(1, sex: Sex.Male, adjustment: AdjustmentType.Premium, premium: 100m),
            TestData.Payment(2, sex: Sex.Male, adjustment: AdjustmentType.Discount, discount: 40m),
            TestData.Payment(3, sex: Sex.Female),
            TestData.Payment(4, sex: Sex.Female, adjustment: AdjustmentType.Discount, discount: 10m));

        Assert.True(result.HasData);
        Assert.Equal(4, result.TotalAnimals);
        Assert.Equal(2, result.Males.Count);
        Assert.Equal(50m, result.Males.Percentage);
        Assert.Equal(2, result.Females.Count);
        Assert.Equal(1, result.Premiums.Count);
        Assert.Equal(25m, result.Premiums.Percentage);
        Assert.Equal(100m, result.Premiums.TotalAmount);
        Assert.Equal(2, result.Discounts.Count);
        Assert.Equal(50m, result.Discounts.TotalAmount);
    }

    [Fact]
    public async Task Execute_GroupsByOrderAndSex_OrderedByOrderThenSex()
    {
        var result = await Execute(
            TestData.Payment(1, orderId: 2, supplier: "B", sex: Sex.Male),
            TestData.Payment(2, orderId: 1, supplier: "A", sex: Sex.Female),
            TestData.Payment(3, orderId: 1, supplier: "A", sex: Sex.Male),
            TestData.Payment(4, orderId: 1, supplier: "A", sex: Sex.Male));

        Assert.Collection(result.OrderSummaries,
            s => Assert.Equal((1, Sex.Male, 2), (s.OrderId, s.Sex, s.Totals.AnimalCount)),
            s => Assert.Equal((1, Sex.Female, 1), (s.OrderId, s.Sex, s.Totals.AnimalCount)),
            s => Assert.Equal((2, Sex.Male, 1), (s.OrderId, s.Sex, s.Totals.AnimalCount)));
    }

    [Fact]
    public async Task Execute_GrandTotalsMatchSumOfSummaries_AndCalculatedAtIsLatest()
    {
        var result = await Execute(
            TestData.Payment(1, orderId: 1, amountToPay: 1000m, calculatedAt: new DateTime(2026, 1, 1)),
            TestData.Payment(2, orderId: 2, amountToPay: 500m, calculatedAt: new DateTime(2026, 3, 1)));

        Assert.Equal(1500m, result.GrandTotals.TotalAmount);
        Assert.Equal(result.GrandTotals.TotalAmount, result.OrderSummaries.Sum(s => s.Totals.TotalAmount));
        Assert.Equal(new DateTime(2026, 3, 1), result.CalculatedAt);
        Assert.Equal(2, result.Animals.Count);
    }
}
