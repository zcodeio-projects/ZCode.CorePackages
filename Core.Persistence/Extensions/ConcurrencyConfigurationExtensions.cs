using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZCode.Core.Domain.Aggregates;

namespace ZCode.Core.Persistence.Extensions;

/// <summary>
/// Extension methods for configuring database-specific concurrency tokens
/// </summary>
public static class ConcurrencyConfigurationExtensions
{
    /// <summary>
    /// Configures optimistic concurrency control using string-based property access
    /// </summary>
    /// <param name="entityBuilder">Entity type builder</param>
    /// <param name="context">Database context for provider detection</param>
    /// <param name="aggregateType">Type of the aggregate</param>
    public static void ConfigureConcurrencyToken(
        this EntityTypeBuilder entityBuilder,
        DbContext context,
        Type aggregateType)
    {
        if (context.IsPostgreSql())
        {
            // PostgreSQL: Use xmin system column
            entityBuilder.Property<uint>("xmin")
                        .HasColumnName("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();
        }
        else if (context.IsSqlServer())
        {
            // SQL Server: Use rowversion/timestamp
            entityBuilder.Property<byte[]>("RowVersion")
                        .IsRowVersion()
                        .HasColumnName("RowVersion");
        }
        else if (context.IsMySql())
        {
            // MySQL: Use timestamp column
            entityBuilder.Property<DateTime>("RowVersion")
                        .HasColumnName("RowVersion")
                        .HasColumnType("timestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate();
        }
        else
        {
            // Fallback: Use integer version for other databases
            entityBuilder.Property("Version")
                        .HasColumnType(typeof(int).Name)
                        .HasColumnName("Version")
                        .IsConcurrencyToken();
        }
    }
}
