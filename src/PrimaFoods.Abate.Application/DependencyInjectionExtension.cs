using Microsoft.Extensions.DependencyInjection;

using PrimaFoods.Abate.Application.UseCases.AnimalPayments.Calculate;
using PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

namespace PrimaFoods.Abate.Application;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ICalculateAnimalPaymentsUseCase, CalculateAnimalPaymentsUseCase>();
        services.AddScoped<IGetPaymentDashboardUseCase, GetPaymentDashboardUseCase>();

        return services;
    }
}
