namespace PrimaFoods.Abate.Application.Models;

/// <summary>Parâmetros de negócio enviados à stored procedure de cálculo. Seção "Payment" da configuração.</summary>
public sealed class PaymentSettings
{
    public const string SectionName = "Payment";

    public decimal MaleArrobaPrice { get; set; } = 100m;

    public decimal FemaleArrobaPrice { get; set; } = 50m;

    public decimal PremiumPercentage { get; set; } = 10m;

    public decimal DiscountPercentage { get; set; } = 10m;

    public bool IsValid(out string error)
    {
        error = string.Empty;

        if (MaleArrobaPrice <= 0 || FemaleArrobaPrice <= 0)
            error = "Os preços da arroba devem ser maiores que zero.";
        else if (PremiumPercentage is < 0 or > 100 || DiscountPercentage is < 0 or > 100)
            error = "Os percentuais de ágio e deságio devem estar entre 0 e 100.";

        return error.Length == 0;
    }
}
