using PrimaFoods.Abate.Domain.Enums;

namespace PrimaFoods.Abate.Domain.Entities;

public sealed class AnimalPayment
{
    public int AnimalId { get; init; }
    public int OrderId { get; init; }
    public string Supplier { get; init; } = string.Empty;
    public Sex Sex { get; init; }
    public int TeethCount { get; init; }
    public decimal Weight { get; init; }
    public decimal Arrobas { get; init; }
    public decimal BaseArrobaPrice { get; init; }
    public decimal BaseAmount { get; init; }
    public decimal PremiumAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal AmountToPay { get; init; }
    public AdjustmentType AdjustmentType { get; init; }
    public string AdjustmentReason { get; init; } = string.Empty;
    public DateTime CalculatedAt { get; init; }
}
