using PrimaFoods.Abate.Domain.Repositories;

namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;

public sealed class CalculateAnimalPaymentsUseCase(IAnimalPaymentRepository repository) : ICalculateAnimalPaymentsUseCase
{
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default) 
        => repository.CalculateAsync(cancellationToken);
}
