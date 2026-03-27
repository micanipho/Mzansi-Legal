using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace backend.EntityFrameworkCore;

/// <summary>
/// Configures the <see cref="backendDbContext"/> to use the Npgsql PostgreSQL provider.
/// Called during application startup and EF Core design-time tooling to register
/// the database provider and connection details on the DbContext options builder.
/// </summary>
public static class backendDbContextConfigurer
{
    /// <summary>
    /// Configures the DbContext to use the Npgsql PostgreSQL provider with a connection string.
    /// </summary>
    public static void Configure(DbContextOptionsBuilder<backendDbContext> builder, string connectionString)
    {
        builder.UseNpgsql(connectionString);
    }

    /// <summary>
    /// Configures the DbContext to use the Npgsql PostgreSQL provider with an existing DbConnection.
    /// </summary>
    public static void Configure(DbContextOptionsBuilder<backendDbContext> builder, DbConnection connection)
    {
        builder.UseNpgsql(connection);
    }
}
