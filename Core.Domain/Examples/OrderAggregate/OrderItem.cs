using ZCode.Core.Domain.Entities;

namespace ZCode.Core.Domain.Examples.OrderAggregate;

/// <summary>
/// Order item entity - part of Order aggregate
/// </summary>
public class OrderItem : Entity<Guid>
{
    public ProductId ProductId { get; private set; }
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public Money TotalPrice { get; private set; }

    // Private constructor for EF Core
    private OrderItem() : base()
    {
        ProductId = null!;
        UnitPrice = null!;
        TotalPrice = null!;
    }

    public OrderItem(ProductId productId, Money unitPrice, int quantity) : base(Guid.NewGuid())
    {
        ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(quantity));

        Quantity = quantity;
        CalculateTotalPrice();
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive", nameof(newQuantity));

        Quantity = newQuantity;
        CalculateTotalPrice();
    }

    public void UpdateUnitPrice(Money newUnitPrice)
    {
        UnitPrice = newUnitPrice ?? throw new ArgumentNullException(nameof(newUnitPrice));
        CalculateTotalPrice();
    }

    private void CalculateTotalPrice()
    {
        TotalPrice = new Money(UnitPrice.Amount * Quantity, UnitPrice.Currency);
    }
}
