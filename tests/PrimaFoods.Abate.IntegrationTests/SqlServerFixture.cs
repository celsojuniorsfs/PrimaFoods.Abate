using System.Text.RegularExpressions;

using Microsoft.Data.SqlClient;

using Microsoft.Extensions.Options;

using PrimaFoods.Abate.Application.Abstractions;
using PrimaFoods.Abate.Application.Models;
using PrimaFoods.Abate.Infrastructure.DataAccess;
using PrimaFoods.Abate.Infrastructure.DataAccess.Payments;

using Testcontainers.MsSql;

namespace PrimaFoods.Abate.IntegrationTests;

/// <summary>
/// Sobe um SQL Server descartável e aplica os scripts de database/ (01 a 04),
/// o mesmo que o container db-init do docker-compose faz.
/// </summary>
public sealed partial class SqlServerFixture : IAsyncLifetime
{
    private const string DatabaseName = "dbRecruta";

    private readonly MsSqlContainer _container = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    private string _connectionString = string.Empty;

    private SqlConnectionFactory _connectionFactory = null!;

    public IPaymentCalculator Calculator => CreateCalculator(new PaymentSettings());

    public IPaymentCalculator CreateCalculator(PaymentSettings settings)
        => new PaymentCalculator(_connectionFactory, Options.Create(settings));

    public IAnimalPaymentReader Reader => new AnimalPaymentReader(_connectionFactory);

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        _connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = DatabaseName
        }.ConnectionString;

        var scripts = Directory.GetFiles(FindDatabaseDirectory(), "*.sql")
            .Where(path => Regex.IsMatch(Path.GetFileName(path), "^0[1-4]_"))
            .Order()
            .ToList();

        Assert.Equal(4, scripts.Count);

        foreach (var script in scripts)
            await RunScriptAsync(await File.ReadAllTextAsync(script));

        _connectionFactory = new SqlConnectionFactory(_connectionString);
    }

    /// <summary>Remove os dados de teste, mantendo o schema criado pelos scripts.</summary>
    public async Task ResetAsync()
    {
        await ExecuteAsync("""
            DELETE FROM dbo.AnimalPayments;
            DELETE FROM dbo.Animals;
            DELETE FROM dbo.Orders;
            """);
    }

    public Task AddOrderAsync(int orderId, string supplier)
        => ExecuteAsync("INSERT INTO dbo.Orders (OrderId, Supplier) VALUES (@id, @supplier)",
            ("@id", orderId), ("@supplier", supplier));

    public Task AddAnimalAsync(int animalId, int? orderId, string? sex, int? teethCount, decimal? weight)
        => ExecuteAsync(
            "INSERT INTO dbo.Animals (AnimalId, Sex, OrderId, TeethCount, Weight) VALUES (@id, @sex, @order, @teeth, @weight)",
            ("@id", animalId), ("@sex", sex), ("@order", orderId), ("@teeth", teethCount), ("@weight", weight));

    private async Task ExecuteAsync(string sql, params (string Name, object? Value)[] parameters)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new SqlCommand(sql, connection);
        foreach (var (name, value) in parameters)
            command.Parameters.AddWithValue(name, value ?? DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    // Os scripts usam GO e USE, que são comandos do sqlcmd e não do servidor: executa lote a lote.
    private async Task RunScriptAsync(string script)
    {
        await using var connection = new SqlConnection(_container.GetConnectionString());
        await connection.OpenAsync();

        foreach (var batch in GoSeparator().Split(script))
        {
            if (string.IsNullOrWhiteSpace(batch))
                continue;

            await using var command = new SqlCommand(batch, connection) { CommandTimeout = 120 };
            await command.ExecuteNonQueryAsync();
        }
    }

    private static string FindDatabaseDirectory()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, "database");
            if (Directory.Exists(candidate))
                return candidate;
        }

        throw new DirectoryNotFoundException("Pasta 'database' não encontrada a partir de " + AppContext.BaseDirectory);
    }

    [GeneratedRegex(@"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase)]
    private static partial Regex GoSeparator();
}

[CollectionDefinition(Name)]
public sealed class SqlServerCollection : ICollectionFixture<SqlServerFixture>
{
    public const string Name = "SqlServer";
}
