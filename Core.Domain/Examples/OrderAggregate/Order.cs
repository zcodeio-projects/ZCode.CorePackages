using ZCode.Core.Domain.Aggregates;
using ZCode.Core.Domain.Events;
using ZCode.Core.Domain.ValueObjects;

namespace ZCode.Core.Domain.Examples.OrderAggregate;

/// <summary>
/// Order aggregate root example demonstrating DDD patterns
/// </summary>
public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _orderItems = new();

    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Address ShippingAddress { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    // Private constructor for EF Core
    private Order() : base()
    {
        CustomerId = null!;
        TotalAmount = null!;
        ShippingAddress = null!;
    }

    public Order(CustomerId customerId, Address shippingAddress) : base(Guid.NewGuid())
    {
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
        TotalAmount = Money.Zero("USD");

        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId.Value, OrderDate));
    }

    public void AddOrderItem(ProductId productId, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        var existingItem = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var orderItem = new OrderItem(productId, unitPrice, quantity);
            _orderItems.Add(orderItem);
        }

        RecalculateTotal();
        AddDomainEvent(new OrderItemAddedEvent(Id, productId.Value, quantity));
    }

    public void RemoveOrderItem(ProductId productId)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot remove items from a non-pending order");

        var item = _orderItems.FirstOrDefault(x => x.ProductId == productId);
        if (item != null)
        {
            _orderItems.Remove(item);
            RecalculateTotal();
            AddDomainEvent(new OrderItemRemovedEvent(Id, productId.Value));
        }
    }

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (!_orderItems.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, TotalAmount.Amount));
    }

    public void CancelOrder()
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel shipped or delivered orders");

        Status = OrderStatus.Cancelled;
        AddDomainEvent(new OrderCancelledEvent(Id));
    }

    public void ShipOrder()
    {
        if (Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed orders can be shipped");

        Status = OrderStatus.Shipped;
        AddDomainEvent(new OrderShippedEvent(Id, ShippingAddress));
    }

    private void RecalculateTotal()
    {
        var total = _orderItems.Sum(item => item.TotalPrice.Amount);
        TotalAmount = new Money(total, TotalAmount.Currency);
    }

    public override bool IsValid()
    {
        return CustomerId != null && 
               ShippingAddress != null && 
               TotalAmount != null &&
               TotalAmount.Amount >= 0;
    }

    public override IEnumerable<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (CustomerId == null)
            errors.Add("Customer ID is required");

        if (ShippingAddress == null)
            errors.Add("Shipping address is required");

        if (TotalAmount == null || TotalAmount.Amount < 0)
            errors.Add("Total amount must be non-negative");

        return errors;
    }
}

// Value Objects
public class CustomerId : ValueObject
{
    public Guid Value { get; }

    public CustomerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Customer ID cannot be empty", nameof(value));
        
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

public class ProductId : ValueObject
{
    public Guid Value { get; }

    public ProductId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(value));
        
        Value = value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }

    public static Money Zero(string currency) => new(0, currency);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}

public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(string street, string city, string state, string zipCode, string country)
    {
        Street = street ?? throw new ArgumentNullException(nameof(street));
        City = city ?? throw new ArgumentNullException(nameof(city));
        State = state ?? throw new ArgumentNullException(nameof(state));
        ZipCode = zipCode ?? throw new ArgumentNullException(nameof(zipCode));
        Country = country ?? throw new ArgumentNullException(nameof(country));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return ZipCode;
        yield return Country;
    }
}

// Enums
public enum OrderStatus
{
    Pending = 0,
    Confirmed = 1,
    Shipped = 2,
    Delivered = 3,
    Cancelled = 4
}
