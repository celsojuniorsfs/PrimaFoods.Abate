using System.Data;

using Dapper;

using Microsoft.Extensions.Options;

using PrimaFoods.Abate.Application.Abstractions;
using PrimaFoods.Abate.Application.Models;

namespace PrimaFoods.Abate.Infrastructure.DataAccess.Payments;

internal sealed class PaymentCalculator(
    SqlConnectionFactory connectionFactory,
    IOptions<PaymentSettings> options) : IPaymentCalculator
{
    public async Task<CalculationResult> CalculateAsync(CancellationToken cancellationToken = default)
    {
        var settings = options.Value;

        var parameters = new DynamicParameters();
        parameters.Add("@MaleArrobaPrice", settings.MaleArrobaPrice, DbType.Decimal);
        parameters.Add("@FemaleArrobaPrice", settings.FemaleArrobaPrice, DbType.Decimal);
        parameters.Add("@PremiumPercentage", settings.PremiumPercentage, DbType.Decimal);
        parameters.Add("@DiscountPercentage", settings.DiscountPercentage, DbType.Decimal);

        var command = new CommandDefinition(
            commandText: "dbo.spCalculateAnimalPayments",
            parameters: parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await using var connection = connectionFactory.Create();

        return await connection.QuerySingleAsync<CalculationResult>(command);
    }
}
