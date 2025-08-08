using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZCode.Core.Domain.Aggregates;

namespace ZCode.Core.Persistence.Extensions;

/// <summary>
/// Extension methods for DbContext to work with shadow properties
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Checks if the database provider is PostgreSQL
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>True if PostgreSQL, false otherwise</returns>
    public static bool IsPostgreSql(this DbContext context)
    {
        return context.Database.ProviderName?.Contains("Npgsql") == true;
    }

    /// <summary>
    /// Checks if the database provider is SQL Server
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>True if SQL Server, false otherwise</returns>
    public static bool IsSqlServer(this DbContext context)
    {
        return context.Database.ProviderName?.Contains("SqlServer") == true;
    }

    /// <summary>
    /// Checks if the database provider is SQLite
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>True if SQLite, false otherwise</returns>
    public static bool IsSqlite(this DbContext context)
    {
        return context.Database.ProviderName?.Contains("Sqlite") == true;
    }

    /// <summary>
    /// Checks if the database provider is MySQL
    /// </summary>
    /// <param name="context">Database context</param>
    /// <returns>True if MySQL, false otherwise</returns>
    public static bool IsMySql(this DbContext context)
    {
        return context.Database.ProviderName?.Contains("MySql") == true ||
               context.Database.ProviderName?.Contains("Pomelo") == true;
    }
}
