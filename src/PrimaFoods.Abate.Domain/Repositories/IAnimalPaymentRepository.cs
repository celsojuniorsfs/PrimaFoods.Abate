using PrimaFoods.Abate.Domain.Entities;

namespace PrimaFoods.Abate.Domain.Repositories;

public interface IAnimalPaymentRepository
{
    Task<int> CalculateAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AnimalPayment>> GetAllAsync(CancellationToken cancellationToken = default);
}
