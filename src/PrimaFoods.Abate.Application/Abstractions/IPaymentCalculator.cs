using PrimaFoods.Abate.Application.Models;

namespace PrimaFoods.Abate.Application.Abstractions;

/// <summary>
/// Executa o cálculo de pagamento. A regra (preço da arroba, ágio e deságio) é de responsabilidade
/// da stored procedure dbo.spCalculateAnimalPayments; veja o README.
/// </summary>
public interface IPaymentCalculator
{
    Task<CalculationResult> CalculateAsync(CancellationToken cancellationToken = default);
}
