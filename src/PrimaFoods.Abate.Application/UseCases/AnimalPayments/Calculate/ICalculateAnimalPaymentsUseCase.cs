using PrimaFoods.Abate.Application.Models;

namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;

public interface ICalculateAnimalPaymentsUseCase
{
    Task<CalculationResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
