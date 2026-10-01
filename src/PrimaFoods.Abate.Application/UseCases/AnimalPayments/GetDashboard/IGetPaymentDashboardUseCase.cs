namespace PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

public interface IGetPaymentDashboardUseCase
{
    Task<PaymentDashboardResult> ExecuteAsync(CancellationToken cancellationToken = default);
}
