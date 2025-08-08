using ZCode.Core.Domain.Examples.OrderAggregate;
using ZCode.Core.Domain.Services;

namespace ZCode.Core.Domain.Examples.Services;

/// <summary>
/// Domain service example for Order aggregate
/// Contains business logic that doesn't naturally fit within an entity or value object
/// </summary>
public class OrderDomainService : IDomainService
{
    public bool CanOrderBeShipped(Order order, DateTime currentDate)
    {
        // Business rule: Orders can only be shipped on weekdays
        if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
            return false;

        // Business rule: Order must be confirmed
        if (order.Status != OrderStatus.Confirmed)
            return false;

        // Business rule: Order must have items
        if (!order.OrderItems.Any())
            return false;

        return true;
    }

    public decimal CalculateShippingCost(Order order, Address destinationAddress)
    {
        // Business logic for calculating shipping cost
        var baseShippingCost = 10.00m;
        var weightMultiplier = order.OrderItems.Count * 2.50m;
        
        // International shipping costs more
        if (destinationAddress.Country != order.ShippingAddress.Country)
        {
            baseShippingCost *= 2;
        }

        // Free shipping for orders over $100
        if (order.TotalAmount.Amount >= 100)
        {
            return 0;
        }

        return baseShippingCost + weightMultiplier;
    }

    public bool IsOrderEligibleForDiscount(Order order, CustomerId customerId)
    {
        // Business rule: Orders over $50 are eligible for discount
        if (order.TotalAmount.Amount < 50)
            return false;

        // Business rule: Only confirmed orders are eligible
        if (order.Status != OrderStatus.Confirmed)
            return false;

        // Additional business logic could check customer loyalty, etc.
        return true;
    }

    public Money CalculateDiscount(Order order, decimal discountPercentage)
    {
        if (!IsOrderEligibleForDiscount(order, order.CustomerId))
            return Money.Zero(order.TotalAmount.Currency);

        var discountAmount = order.TotalAmount.Amount * (discountPercentage / 100);
        return new Money(discountAmount, order.TotalAmount.Currency);
    }

    public bool ValidateOrderBusinessRules(Order order)
    {
        // Aggregate business rules validation
        var errors = new List<string>();

        // Rule: Order must have at least one item
        if (!order.OrderItems.Any())
            errors.Add("Order must contain at least one item");

        // Rule: All items must have positive quantity
        if (order.OrderItems.Any(item => item.Quantity <= 0))
            errors.Add("All order items must have positive quantity");

        // Rule: Total amount must match sum of item totals
        var calculatedTotal = order.OrderItems.Sum(item => item.TotalPrice.Amount);
        if (Math.Abs(order.TotalAmount.Amount - calculatedTotal) > 0.01m)
            errors.Add("Order total does not match sum of item totals");

        return !errors.Any();
    }
}
