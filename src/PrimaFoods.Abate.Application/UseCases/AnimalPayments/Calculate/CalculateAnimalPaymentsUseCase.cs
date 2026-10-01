using PrimaFoods.Abate.Application.Abstractions;
using PrimaFoods.Abate.Application.Models;

namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;

public sealed class CalculateAnimalPaymentsUseCase(IPaymentCalculator calculator) : ICalculateAnimalPaymentsUseCase
{
    public Task<CalculationResult> ExecuteAsync(CancellationToken cancellationToken = default)
        => calculator.CalculateAsync(cancellationToken);
}
