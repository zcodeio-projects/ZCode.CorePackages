using ZCode.Core.Domain.Entities;

namespace ZCode.Core.Domain.Aggregates;

/// <summary>
/// Marker interface for aggregate roots in Domain-Driven Design.
/// An aggregate root is the only member of its aggregate that outside objects are allowed to hold references to.
/// </summary>
public interface IAggregateRoot
{
}

/// <summary>
/// Generic aggregate root interface with strongly typed ID
/// </summary>
/// <typeparam name="TId">Type of the aggregate root identifier</typeparam>
public interface IAggregateRoot<TId> : IAggregateRoot, IEntity<TId>
{
}
