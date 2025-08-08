using ZCode.Core.Domain.Events;

namespace ZCode.Core.Domain.Examples.OrderAggregate;

/// <summary>
/// Domain events for Order aggregate
/// </summary>

public record OrderCreatedEvent(Guid OrderId, Guid CustomerId, DateTime OrderDate) : IDomainEvent;

public record OrderItemAddedEvent(Guid OrderId, Guid ProductId, int Quantity) : IDomainEvent;

public record OrderItemRemovedEvent(Guid OrderId, Guid ProductId) : IDomainEvent;

public record OrderConfirmedEvent(Guid OrderId, decimal TotalAmount) : IDomainEvent;

public record OrderCancelledEvent(Guid OrderId) : IDomainEvent;

public record OrderShippedEvent(Guid OrderId, Address ShippingAddress) : IDomainEvent;
