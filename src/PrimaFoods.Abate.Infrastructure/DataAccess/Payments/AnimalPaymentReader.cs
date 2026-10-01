using Dapper;

using PrimaFoods.Abate.Application.Abstractions;
using PrimaFoods.Abate.Application.Models;

namespace PrimaFoods.Abate.Infrastructure.DataAccess.Payments;

internal sealed class AnimalPaymentReader(SqlConnectionFactory connectionFactory) : IAnimalPaymentReader
{
    public async Task<IReadOnlyList<AnimalPaymentView>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                 p.AnimalId
                ,p.OrderId
                ,o.Supplier
                ,p.Sex
                ,p.TeethCount
                ,p.Weight
                ,p.Arrobas
                ,p.BaseArrobaPrice
                ,p.BaseAmount
                ,p.PremiumAmount
                ,p.DiscountAmount
                ,p.AmountToPay
                ,p.AdjustmentType
                ,p.AdjustmentReason
                ,p.CalculatedAt
            FROM dbo.AnimalPayments AS p
            INNER JOIN dbo.Orders AS o ON o.OrderId = p.OrderId
            ORDER BY p.OrderId, p.AnimalId;
            """;

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        await using var connection = connectionFactory.Create();

        var rows = await connection.QueryAsync<Row>(command);

        return rows.Select(row => row.ToView()).ToList();
    }

    private sealed class Row
    {
        public int AnimalId { get; init; }
        public int OrderId { get; init; }
        public string Supplier { get; init; } = string.Empty;
        public string Sex { get; init; } = string.Empty;
        public int TeethCount { get; init; }
        public decimal Weight { get; init; }
        public decimal Arrobas { get; init; }
        public decimal BaseArrobaPrice { get; init; }
        public decimal BaseAmount { get; init; }
        public decimal PremiumAmount { get; init; }
        public decimal DiscountAmount { get; init; }
        public decimal AmountToPay { get; init; }
        public string AdjustmentType { get; init; } = string.Empty;
        public string AdjustmentReason { get; init; } = string.Empty;
        public DateTime CalculatedAt { get; init; }

        public AnimalPaymentView ToView() => new()
        {
            AnimalId = AnimalId,
            OrderId = OrderId,
            Supplier = Supplier,
            Sex = DbCodes.ToSex(Sex),
            TeethCount = TeethCount,
            Weight = Weight,
            Arrobas = Arrobas,
            BaseArrobaPrice = BaseArrobaPrice,
            BaseAmount = BaseAmount,
            PremiumAmount = PremiumAmount,
            DiscountAmount = DiscountAmount,
            AmountToPay = AmountToPay,
            AdjustmentType = DbCodes.ToAdjustmentType(AdjustmentType),
            AdjustmentReason = AdjustmentReason,
            CalculatedAt = CalculatedAt
        };
    }
}
