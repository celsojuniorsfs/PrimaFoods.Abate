using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

public sealed record OrderSummary(int OrderId, string Supplier, Sex Sex, PaymentTotals Totals);
