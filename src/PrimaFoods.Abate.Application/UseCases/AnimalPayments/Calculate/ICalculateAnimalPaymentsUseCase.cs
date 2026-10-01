namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;

public interface ICalculateAnimalPaymentsUseCase
{
    Task<int> ExecuteAsync(CancellationToken cancellationToken = default);
}
