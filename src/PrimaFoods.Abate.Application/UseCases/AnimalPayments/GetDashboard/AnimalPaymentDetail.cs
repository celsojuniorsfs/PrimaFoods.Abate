using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

public sealed record AnimalPaymentDetail(
    int AnimalId,
    int OrderId,
    string Supplier,
    Sex Sex,
    int TeethCount,
    decimal Weight,
    decimal Arrobas,
    decimal BaseArrobaPrice,
    decimal BaseAmount,
    decimal PremiumAmount,
    decimal DiscountAmount,
    decimal AmountToPay,
    AdjustmentType AdjustmentType,
    string AdjustmentReason)
{
    public static AnimalPaymentDetail From(AnimalPaymentView payment) => new(
        payment.AnimalId,
        payment.OrderId,
        payment.Supplier,
        payment.Sex,
        payment.TeethCount,
        payment.Weight,
        payment.Arrobas,
        payment.BaseArrobaPrice,
        payment.BaseAmount,
        payment.PremiumAmount,
        payment.DiscountAmount,
        payment.AmountToPay,
        payment.AdjustmentType,
        payment.AdjustmentReason
    );
}
