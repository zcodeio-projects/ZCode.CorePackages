using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZCode.Core.Domain.Aggregates;
using ZCode.Core.Persistence.Extensions;

namespace ZCode.Core.Persistence.Configurations;

/// <summary>
/// Global configuration for all aggregate roots
/// Automatically configures database-specific concurrency control
/// </summary>
/// <typeparam name="TAggregate">Type of aggregate root</typeparam>
/// <typeparam name="TId">Type of aggregate ID</typeparam>
public abstract class AggregateRootConfiguration<TAggregate, TId> : IEntityTypeConfiguration<TAggregate>
    where TAggregate : class, IAggregateRoot<TId>
    where TId : IEquatable<TId>
{
    protected readonly DbContext Context;

    protected AggregateRootConfiguration(DbContext context)
    {
        Context = context;
    }

    public virtual void Configure(EntityTypeBuilder<TAggregate> builder)
    {
        // Configure primary key
        builder.HasKey(x => x.Id);

        // Configure database-specific concurrency control
        builder.ConfigureConcurrencyToken<TAggregate, TId>(Context);

        // Configure audit fields if aggregate implements IAuditableEntity
        ConfigureAuditFields(builder);

        // Configure domain events (ignore from database)
        builder.Ignore("DomainEvents");

        // Call derived configuration
        ConfigureAggregate(builder);
    }

    /// <summary>
    /// Override this method to configure specific aggregate properties
    /// </summary>
    /// <param name="builder">Entity type builder</param>
    protected abstract void ConfigureAggregate(EntityTypeBuilder<TAggregate> builder);

    /// <summary>
    /// Configures audit fields if the aggregate implements audit interfaces
    /// </summary>
    private void ConfigureAuditFields(EntityTypeBuilder<TAggregate> builder)
    {
        var aggregateType = typeof(TAggregate);

        // Check if aggregate implements ICreatedByEntity
        if (aggregateType.GetInterfaces().Any(i => i.Name.Contains("ICreatedByEntity")))
        {
            builder.Property("CreatedByUserId")
                   .HasColumnName("CreatedByUserId");

            builder.Property("CreatedDate")
                   .HasColumnName("CreatedDate")
                   .IsRequired();
        }

        // Check if aggregate implements IUpdatedByEntity
        if (aggregateType.GetInterfaces().Any(i => i.Name.Contains("IUpdatedByEntity")))
        {
            builder.Property("UpdatedByUserId")
                   .HasColumnName("UpdatedByUserId");

            builder.Property("UpdatedDate")
                   .HasColumnName("UpdatedDate");
        }

        // Check if aggregate implements IDeletedByEntity
        if (aggregateType.GetInterfaces().Any(i => i.Name.Contains("IDeletedByEntity")))
        {
            builder.Property("DeletedByUserId")
                   .HasColumnName("DeletedByUserId");

            builder.Property("DeletedDate")
                   .HasColumnName("DeletedDate");

            // Configure soft delete filter
            builder.HasQueryFilter(e => EF.Property<DateTime?>(e, "DeletedDate") == null);
        }
    }
}
