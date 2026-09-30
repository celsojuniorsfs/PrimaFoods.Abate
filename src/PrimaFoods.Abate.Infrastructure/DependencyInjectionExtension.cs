using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PrimaFoods.Abate.Domain.Repositories;
using PrimaFoods.Abate.Infrastructure.DataAccess;
using PrimaFoods.Abate.Infrastructure.DataAccess.Repositories;

namespace PrimaFoods.Abate.Infrastructure;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Connection")
            ?? throw new InvalidOperationException("A connection string 'Connection' não foi configurada.");

        services.AddSingleton(new SqlConnectionFactory(connectionString));
        services.AddScoped<IAnimalPaymentRepository, AnimalPaymentRepository>();

        return services;
    }
}
