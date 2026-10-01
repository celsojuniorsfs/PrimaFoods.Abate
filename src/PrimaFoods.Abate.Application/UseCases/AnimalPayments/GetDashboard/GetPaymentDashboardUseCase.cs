using PrimaFoods.Abate.Application.Common;
using PrimaFoods.Abate.Domain.Enums;
using PrimaFoods.Abate.Application.Abstractions;

namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

public sealed class GetPaymentDashboardUseCase(IAnimalPaymentReader reader) : IGetPaymentDashboardUseCase
{
    public async Task<PaymentDashboardResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var payments = await reader.GetAllAsync(cancellationToken);
        var totalAnimals = payments.Count;

        var males = payments.Count(p => p.Sex == Sex.Male);
        var females = payments.Count(p => p.Sex == Sex.Female);

        var premiums = payments.Where(p => p.AdjustmentType == AdjustmentType.Premium).ToList();
        var discounts = payments.Where(p => p.AdjustmentType == AdjustmentType.Discount).ToList();

        var orderSummaries = payments
            .GroupBy(p => new { p.OrderId, p.Supplier, p.Sex })
            .OrderBy(group => group.Key.OrderId)
            .ThenBy(group => group.Key.Sex)
            .Select(group => new OrderSummary(
                group.Key.OrderId,
                group.Key.Supplier,
                group.Key.Sex,
                PaymentTotals.From(group.ToList())))
            .ToList();

        return new PaymentDashboardResult(
            TotalAnimals: totalAnimals,
            Males: new CountIndicator(males, Calculations.Percentage(males, totalAnimals)),
            Females: new CountIndicator(females, Calculations.Percentage(females, totalAnimals)),
            Premiums: new AdjustmentIndicator(
                premiums.Count,
                Calculations.Percentage(premiums.Count, totalAnimals),
                premiums.Sum(p => p.PremiumAmount)),
            Discounts: new AdjustmentIndicator(
                discounts.Count,
                Calculations.Percentage(discounts.Count, totalAnimals),
                discounts.Sum(p => p.DiscountAmount)),
            OrderSummaries: orderSummaries,
            GrandTotals: PaymentTotals.From(payments),
            Animals: payments.Select(AnimalPaymentDetail.From).ToList(),
            CalculatedAt: totalAnimals > 0 ? payments.Max(p => p.CalculatedAt) : null);
    }
}
