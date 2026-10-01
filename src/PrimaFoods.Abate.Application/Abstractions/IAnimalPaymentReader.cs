using PrimaFoods.Abate.Application.Models;

namespace PrimaFoods.Abate.Application.Abstractions;

public interface IAnimalPaymentReader
{
    Task<IReadOnlyList<AnimalPaymentView>> GetAllAsync(CancellationToken cancellationToken = default);
}
