using Microsoft.Data.SqlClient;

namespace PrimaFoods.Abate.Infrastructure.DataAccess;

internal sealed class SqlConnectionFactory(string connectionString)
{
    public SqlConnection Create() => new(connectionString);
}
