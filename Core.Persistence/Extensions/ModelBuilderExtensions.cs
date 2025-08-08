using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using ZCode.Core.Domain.Aggregates;
using ZCode.Core.Domain.Entities;

namespace ZCode.Core.Persistence.Extensions;

public static class ModelBuilderExtensions
{
    public static void RegisterAllEntities<TInterface>(this ModelBuilder modelBuilder, Assembly assembly)
    {
        var entityTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(TInterface).IsAssignableFrom(t));

        foreach (var type in entityTypes)
        {
            modelBuilder.Entity(type);
        }
    }

    public static void ApplySoftDeleteQueryFilter(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IEntityTimestamps).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(IEntityTimestamps.DeletedDate));
                var condition = Expression.Equal(property, Expression.Constant(null));
                var lambda = Expression.Lambda(condition, parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public static void IgnoreDomainEvenProperty(this ModelBuilder modelBuilder)
    {
        // Ignore DomainEvents property
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.IsAssignableFrom(typeof(IHasDomainEvents)))
                modelBuilder.Ignore("DomainEvents");
        }
    }
    /// <summary>
    /// Automatically configures all aggregate roots with database-specific optimistic concurrency control
    /// </summary>
    public static void ConfigureAggregateRoots(this ModelBuilder modelBuilder, DbContext context)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if entity implements IAggregateRoot
            if (entityType.ClrType.IsAssignableFrom(typeof(IAggregateRoot<>)))
                ConfigureAggregateRoot(modelBuilder, entityType.ClrType, context);
            // Configure audit fields if applicable



            // Configure audit fields if applicable
            else if (entityType.ClrType.IsAssignableFrom(typeof()))
                ConfigureAuditFieldsForType(modelBuilder, aggregateType);
        }
    }

    /// <summary>
    /// Configures a specific aggregate root type with database-specific concurrency control
    /// </summary>
    private static void ConfigureAggregateRoot(ModelBuilder modelBuilder, Type aggregateType, DbContext context)
    {
        var entityBuilder = modelBuilder.Entity(aggregateType);

        // Configure database-specific concurrency control
        entityBuilder.ConfigureConcurrencyToken(context, aggregateType);
    }



    /// <summary>
    /// Configures audit fields for a specific type
    /// </summary>
    private static void ConfigureAuditFieldsForType(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder entityBuilder, Type aggregateType)
    {
        // Check if aggregate implements ICreatedByEntity
        if (aggregateType.GetInterfaces().Any(i => i.Name.Contains("ICreatedByEntity")))
        {
            entityBuilder.Property("CreatedByUserId")
                        .HasColumnName("CreatedByUserId");

            entityBuilder.Property("CreatedDate")
                        .HasColumnName("CreatedDate")
                        .IsRequired();
        }

        // Check if aggregate implements IUpdatedByEntity
        if (aggregateType.GetInterfaces().Any(i => i.Name.Contains("IUpdatedByEntity")))
        {
            entityBuilder.Property("UpdatedByUserId")
                        .HasColumnName("UpdatedByUserId");

            entityBuilder.Property("UpdatedDate")
                        .HasColumnName("UpdatedDate");
        }

        // Check if aggregate implements IDeletedByEntity
        if (aggregateType.GetInterfaces().Any(i => i.Name.Contains("IDeletedByEntity")))
        {
            entityBuilder.Property("DeletedByUserId")
                        .HasColumnName("DeletedByUserId");

            entityBuilder.Property("DeletedDate")
                        .HasColumnName("DeletedDate");

            // Configure soft delete filter
            var parameter = Expression.Parameter(aggregateType, "e");
            var property = Expression.Property(parameter, "DeletedDate");
            var condition = Expression.Equal(property, Expression.Constant(null, typeof(DateTime?)));
            var lambda = Expression.Lambda(condition, parameter);

            entityBuilder.HasQueryFilter(lambda);
        }
    }
}
