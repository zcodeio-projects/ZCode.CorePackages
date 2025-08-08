using ZCode.Core.Domain.Entities;
using ZCode.Core.Domain.Events;

namespace ZCode.Core.Domain.Aggregates;

/// <summary>
/// Base class for aggregate roots in Domain-Driven Design.
/// Provides domain event management and ensures aggregate consistency.
/// </summary>
/// <typeparam name="TId">Type of the aggregate root identifier</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IAggregateRoot<TId>
{


    /// <summary>
    /// Version property for databases that don't support native row versioning (fallback)
    /// </summary>
    public int Version { get; set; }

    protected AggregateRoot() : base()
    {
    }

    protected AggregateRoot(TId id) : base(id)
    {
    }

    /// <summary>
    /// Adds a domain event (RowVersion is automatically managed by database)
    /// </summary>
    /// <param name="domainEvent">Domain event to add</param>
    public new void AddDomainEvent(IDomainEvent domainEvent)
    {
        base.AddDomainEvent(domainEvent);
        // RowVersion is automatically incremented by SQL Server
    }

    /// <summary>
    /// Validates the aggregate's business rules
    /// Override this method to implement aggregate-specific validation
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public virtual bool IsValid()
    {
        return true;
    }

    /// <summary>
    /// Gets validation errors for the aggregate
    /// Override this method to provide specific validation messages
    /// </summary>
    /// <returns>Collection of validation error messages</returns>
    public virtual IEnumerable<string> GetValidationErrors()
    {
        return [];
    }
}
