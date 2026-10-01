using PrimaFoods.Abate.Application.Common;
using PrimaFoods.Abate.Domain.Entities;

namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

public sealed record PaymentTotals(
    int AnimalCount,
    decimal TotalWeight,
    decimal TotalArrobas,
    decimal TotalPremium,
    decimal TotalDiscount,
    decimal TotalAmount,
    decimal ArrobaUnitPrice)
{
    public static PaymentTotals From(IReadOnlyCollection<AnimalPayment> payments)
    {
        var totalArrobas = payments.Sum(p => p.Arrobas);
        var totalAmount = payments.Sum(p => p.AmountToPay);

        return new PaymentTotals(
            AnimalCount: payments.Count,
            TotalWeight: payments.Sum(p => p.Weight),
            TotalArrobas: totalArrobas,
            TotalPremium: payments.Sum(p => p.PremiumAmount),
            TotalDiscount: payments.Sum(p => p.DiscountAmount),
            TotalAmount: totalAmount,
            ArrobaUnitPrice: Calculations.Divide(totalAmount, totalArrobas)
        );
    }
}
