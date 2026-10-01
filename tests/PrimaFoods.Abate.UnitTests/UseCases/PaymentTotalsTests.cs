using PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

namespace PrimaFoods.Abate.UnitTests.UseCases;

public class PaymentTotalsTests
{
    [Fact]
    public void From_EmptyList_ReturnsZeroesWithoutDividingByZero()
    {
        var totals = PaymentTotals.From([]);

        Assert.Equal(0, totals.AnimalCount);
        Assert.Equal(0m, totals.TotalAmount);
        Assert.Equal(0m, totals.ArrobaUnitPrice);
    }

    [Fact]
    public void From_SumsAllFieldsAndComputesUnitPrice()
    {
        var totals = PaymentTotals.From([
            TestData.Payment(weight: 300m, arrobas: 20m, premium: 100m, amountToPay: 2100m),
            TestData.Payment(animalId: 2, weight: 150m, arrobas: 10m, discount: 50m, amountToPay: 450m)
        ]);

        Assert.Equal(2, totals.AnimalCount);
        Assert.Equal(450m, totals.TotalWeight);
        Assert.Equal(30m, totals.TotalArrobas);
        Assert.Equal(100m, totals.TotalPremium);
        Assert.Equal(50m, totals.TotalDiscount);
        Assert.Equal(2550m, totals.TotalAmount);
        Assert.Equal(85m, totals.ArrobaUnitPrice);
    }
}
