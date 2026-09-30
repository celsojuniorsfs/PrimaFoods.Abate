using System.Data;

using Dapper;

using PrimaFoods.Abate.Domain.Entities;
using PrimaFoods.Abate.Domain.Repositories;

namespace PrimaFoods.Abate.Infrastructure.DataAccess.Repositories;

internal sealed class AnimalPaymentRepository(SqlConnectionFactory connectionFactory) : IAnimalPaymentRepository
{
    public async Task<int> CalculateAsync(CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(
            commandText: "dbo.spCalculateAnimalPayments",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        await using var connection = connectionFactory.Create();

        return await connection.ExecuteScalarAsync<int>(command);
    }

    public async Task<IReadOnlyList<AnimalPayment>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                 p.AnimalId
                ,p.OrderId
                ,o.Supplier
                ,CASE p.Sex WHEN 'M' THEN 1 ELSE 2 END AS Sex
                ,p.TeethCount
                ,p.Weight
                ,p.Arrobas
                ,p.BaseArrobaPrice
                ,p.BaseAmount
                ,p.PremiumAmount
                ,p.DiscountAmount
                ,p.AmountToPay
                ,CASE p.AdjustmentType WHEN 'A' THEN 1 WHEN 'D' THEN 2 ELSE 0 END AS AdjustmentType
                ,p.AdjustmentReason
                ,p.CalculatedAt
            FROM dbo.AnimalPayments AS p
            INNER JOIN dbo.Orders AS o ON o.OrderId = p.OrderId
            ORDER BY p.OrderId, p.AnimalId;
            """;

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        await using var connection = connectionFactory.Create();

        var payments = await connection.QueryAsync<AnimalPayment>(command);

        return payments.AsList();
    }
}
