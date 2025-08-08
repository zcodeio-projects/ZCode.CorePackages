using Microsoft.EntityFrameworkCore;
using ZCode.Core.Domain.Aggregates;
using ZCode.Core.Persistence.Extensions;
using ZCode.Core.Persistence.Interceptors;

namespace ZCode.Core.Persistence.Contexts;

/// <summary>
/// Base DbContext that automatically configures aggregate roots and applies DDD patterns
/// </summary>
public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

        // Automatically configure all aggregate roots with database-specific concurrency
        modelBuilder.ConfigureAggregateRoots(this);

        // Apply soft delete query filters
        modelBuilder.ApplySoftDeleteQueryFilter();

        // Register all entities that implement IEntity
        modelBuilder.RegisterAllEntities<IEntity>(GetType().Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Add interceptors for domain events and auditing
        optionsBuilder.AddInterceptors(
            new DomainEventsInterceptor(),
            new AuditableEntitySaveChangesInterceptors<Guid>()
        );
    }

    /// <summary>
    /// Override SaveChanges to handle optimistic concurrency
    /// </summary>
    public override int SaveChanges()
    {
        try
        {
            return base.SaveChanges();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new Core.Persistence.Exceptions.ConcurrencyException(
                "A concurrency conflict occurred while saving changes.", ex);
        }
    }

    /// <summary>
    /// Override SaveChangesAsync to handle optimistic concurrency
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new Core.Persistence.Exceptions.ConcurrencyException(
                "A concurrency conflict occurred while saving changes.", ex);
        }
    }
}
