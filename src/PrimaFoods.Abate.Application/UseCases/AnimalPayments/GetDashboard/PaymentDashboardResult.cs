namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

public sealed record PaymentDashboardResult(
    int TotalAnimals,
    CountIndicator Males,
    CountIndicator Females,
    AdjustmentIndicator Premiums,
    AdjustmentIndicator Discounts,
    IReadOnlyList<OrderSummary> OrderSummaries,
    PaymentTotals GrandTotals,
    IReadOnlyList<AnimalPaymentDetail> Animals,
    DateTime? CalculatedAt)
{
    public bool HasData => TotalAnimals > 0;
}
