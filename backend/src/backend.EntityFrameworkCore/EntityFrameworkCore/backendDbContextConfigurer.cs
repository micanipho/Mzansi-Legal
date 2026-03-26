using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace backend.EntityFrameworkCore;

public static class backendDbContextConfigurer
{
    public static void Configure(DbContextOptionsBuilder<backendDbContext> builder, string connectionString)
    {
        builder.UseSqlServer(connectionString);
    }

    public static void Configure(DbContextOptionsBuilder<backendDbContext> builder, DbConnection connection)
    {
        builder.UseSqlServer(connection);
    }
}


