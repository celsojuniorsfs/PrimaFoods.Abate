using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using PrimaFoods.Abate.Application.Abstractions;
using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Infrastructure.DataAccess;
using PrimaFoods.Abate.Infrastructure.DataAccess.Payments;

namespace PrimaFoods.Abate.Infrastructure;

public static class DependencyInjectionExtension
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Connection")
            ?? throw new InvalidOperationException("A connection string 'Connection' não foi configurada.");

        services.AddOptions<PaymentSettings>()
            .Bind(configuration.GetSection(PaymentSettings.SectionName))
            .Validate(settings => settings.IsValid(out _), "A seção 'Payment' da configuração é inválida: preços da arroba devem ser positivos e percentuais ficar entre 0 e 100.")
            .ValidateOnStart();

        services.AddSingleton(new SqlConnectionFactory(connectionString));
        services.AddScoped<IAnimalPaymentReader, AnimalPaymentReader>();
        services.AddScoped<IPaymentCalculator, PaymentCalculator>();

        return services;
    }
}
