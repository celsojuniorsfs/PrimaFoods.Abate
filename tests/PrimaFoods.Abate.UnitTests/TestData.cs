using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.UnitTests;

internal static class TestData
{
    public static AnimalPaymentView Payment(
        int animalId = 1,
        int orderId = 1,
        string supplier = "Fornecedor",
        Sex sex = Sex.Male,
        decimal weight = 300m,
        decimal arrobas = 20m,
        decimal premium = 0m,
        decimal discount = 0m,
        decimal amountToPay = 2000m,
        AdjustmentType adjustment = AdjustmentType.None,
        DateTime? calculatedAt = null) => new()
    {
        AnimalId = animalId,
        OrderId = orderId,
        Supplier = supplier,
        Sex = sex,
        TeethCount = 2,
        Weight = weight,
        Arrobas = arrobas,
        PremiumAmount = premium,
        DiscountAmount = discount,
        AmountToPay = amountToPay,
        AdjustmentType = adjustment,
        CalculatedAt = calculatedAt ?? new DateTime(2026, 1, 1)
    };
}
