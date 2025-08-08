using ZCode.Core.Domain.Examples.OrderAggregate;
using ZCode.Core.Persistence.Repositories;
using ZCode.Core.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;

namespace ZCode.Core.Persistence.Examples;

/// <summary>
/// Example service showing how to use shadow RowVersion for optimistic concurrency
/// </summary>
public class OrderService
{
    private readonly IAggregateRepository<Order, Guid> _orderRepository;
    private readonly DbContext _context;

    public OrderService(IAggregateRepository<Order, Guid> orderRepository, DbContext context)
    {
        _orderRepository = orderRepository;
        _context = context;
    }

    /// <summary>
    /// Updates an order with optimistic concurrency control using shadow RowVersion
    /// </summary>
    public async Task UpdateOrderAsync(Guid orderId, UpdateOrderCommand command)
    {
        // 1. Get order
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) 
            throw new InvalidOperationException("Order not found");

        // 2. Get current RowVersion from shadow property
        var currentRowVersion = _context.GetRowVersion<Order, Guid>(order);

        // 3. Apply business logic
        if (command.NewShippingAddress != null)
        {
            // Assuming Order has an UpdateShippingAddress method
            // order.UpdateShippingAddress(command.NewShippingAddress);
        }

        if (command.OrderItems?.Any() == true)
        {
            foreach (var item in command.OrderItems)
            {
                order.AddOrderItem(
                    new ProductId(item.ProductId), 
                    new Money(item.UnitPrice, "USD"), 
                    item.Quantity);
            }
        }

        // 4. Save with optimistic concurrency check
        try
        {
            await _orderRepository.SaveAsync(order, currentRowVersion);
        }
        catch (Core.Persistence.Exceptions.ConcurrencyException)
        {
            throw new InvalidOperationException("Order was modified by another user. Please refresh and try again.");
        }
    }

    /// <summary>
    /// Gets an order with its current RowVersion
    /// </summary>
    public async Task<OrderWithRowVersionDto> GetOrderWithRowVersionAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) 
            return null;

        var rowVersion = _context.GetRowVersion<Order, Guid>(order);

        return new OrderWithRowVersionDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId.Value,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            Currency = order.TotalAmount.Currency,
            OrderDate = order.OrderDate,
            RowVersion = rowVersion,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId.Value,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice.Amount,
                TotalPrice = oi.TotalPrice.Amount
            }).ToList()
        };
    }

    /// <summary>
    /// Demonstrates checking if RowVersion was modified
    /// </summary>
    public async Task<bool> CheckIfOrderWasModifiedAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return false;

        // Check if the shadow RowVersion property has been modified
        return _context.IsRowVersionModified<Order, Guid>(order);
    }
}

// DTOs
public class UpdateOrderCommand
{
    public Address? NewShippingAddress { get; set; }
    public List<OrderItemCommand>? OrderItems { get; set; }
}

public class OrderItemCommand
{
    public Guid ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

public class OrderWithRowVersionDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public uint RowVersion { get; set; } // Shadow property value
    public List<OrderItemDto> OrderItems { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
