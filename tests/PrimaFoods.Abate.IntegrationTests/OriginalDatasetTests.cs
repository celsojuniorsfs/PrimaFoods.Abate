using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Application.UseCases.AnimalPayments.GetDashboard;

namespace PrimaFoods.Abate.IntegrationTests;

[CollectionDefinition(Name)]
public sealed class OriginalDatasetCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "OriginalDataset";
}

/// <summary>
/// Calcula sobre a massa de dados do script fornecido no teste (1.128 animais), sem limpar a base,
/// e confere os números que a tela deve exibir.
/// </summary>
[Collection(OriginalDatasetCollection.Name)]
public sealed class OriginalDatasetTests(SqlServerFixture db)
{
    [Fact]
    public async Task Calculate_WithOriginalScript_MatchesReferenceTotals()
    {
        var result = await db.Calculator.CalculateAsync();
        var dashboard = await new GetPaymentDashboardUseCase(db.Reader).ExecuteAsync();

        Assert.Equal(new CalculationResult(ProcessedAnimals: 1128, SkippedAnimals: 0), result);

        Assert.Equal(1110, dashboard.Males.Count);
        Assert.Equal(98.40m, dashboard.Males.Percentage);
        Assert.Equal(18, dashboard.Females.Count);
        Assert.Equal(1.60m, dashboard.Females.Percentage);

        Assert.Equal(199, dashboard.Premiums.Count);
        Assert.Equal(17.64m, dashboard.Premiums.Percentage);
        Assert.Equal(36854.01m, dashboard.Premiums.TotalAmount);

        Assert.Equal(188, dashboard.Discounts.Count);
        Assert.Equal(16.67m, dashboard.Discounts.Percentage);
        Assert.Equal(35813.85m, dashboard.Discounts.TotalAmount);

        Assert.Equal(18, dashboard.OrderSummaries.Count);
        Assert.Equal(2134981.91m, dashboard.GrandTotals.TotalAmount);
        Assert.Equal(99.33m, dashboard.GrandTotals.ArrobaUnitPrice);
    }
}
