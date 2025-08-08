using ZCode.Core.Domain.Specifications;

namespace ZCode.Core.Domain.Examples.OrderAggregate;

/// <summary>
/// Specifications for Order aggregate
/// </summary>

public class OrdersByCustomerSpecification : BaseSpecification<Order>
{
    public OrdersByCustomerSpecification(Guid customerId)
        : base(order => order.CustomerId.Value == customerId)
    {
        AddInclude(order => order.OrderItems);
        AddOrderByDescending(order => order.OrderDate);
    }
}

public class OrdersByStatusSpecification : BaseSpecification<Order>
{
    public OrdersByStatusSpecification(OrderStatus status)
        : base(order => order.Status == status)
    {
        AddInclude(order => order.OrderItems);
        AddOrderBy(order => order.OrderDate);
    }
}

public class OrdersByDateRangeSpecification : BaseSpecification<Order>
{
    public OrdersByDateRangeSpecification(DateTime startDate, DateTime endDate)
        : base(order => order.OrderDate >= startDate && order.OrderDate <= endDate)
    {
        AddInclude(order => order.OrderItems);
        AddOrderByDescending(order => order.OrderDate);
    }
}

public class OrdersWithMinimumAmountSpecification : BaseSpecification<Order>
{
    public OrdersWithMinimumAmountSpecification(decimal minimumAmount, string currency)
        : base(order => order.TotalAmount.Amount >= minimumAmount && order.TotalAmount.Currency == currency)
    {
        AddInclude(order => order.OrderItems);
        AddOrderByDescending(order => order.TotalAmount.Amount);
    }
}

public class PendingOrdersSpecification : BaseSpecification<Order>
{
    public PendingOrdersSpecification()
        : base(order => order.Status == OrderStatus.Pending)
    {
        AddInclude(order => order.OrderItems);
        AddOrderBy(order => order.OrderDate);
    }
}

public class OrdersReadyToShipSpecification : BaseSpecification<Order>
{
    public OrdersReadyToShipSpecification()
        : base(order => order.Status == OrderStatus.Confirmed)
    {
        AddInclude(order => order.OrderItems);
        AddOrderBy(order => order.OrderDate);
    }
}
