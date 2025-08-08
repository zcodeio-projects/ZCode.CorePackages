using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZCode.Core.Domain.Examples.OrderAggregate;

namespace ZCode.Core.Persistence.Configurations.Examples;

/// <summary>
/// Example configuration for Order aggregate using the base AggregateRootConfiguration
/// Version property is automatically configured by the base class
/// </summary>
public class OrderConfiguration : AggregateRootConfiguration<Order, Guid>
{
    protected override void ConfigureAggregate(EntityTypeBuilder<Order> builder)
    {
        // Table name
        builder.ToTable("Orders");

        // Value object configurations
        builder.OwnsOne(o => o.CustomerId, cb =>
        {
            cb.Property(c => c.Value)
              .HasColumnName("CustomerId")
              .IsRequired();
        });

        builder.OwnsOne(o => o.TotalAmount, mb =>
        {
            mb.Property(m => m.Amount)
              .HasColumnName("TotalAmount")
              .HasColumnType("decimal(18,2)")
              .IsRequired();

            mb.Property(m => m.Currency)
              .HasColumnName("Currency")
              .HasMaxLength(3)
              .IsRequired();
        });

        builder.OwnsOne(o => o.ShippingAddress, ab =>
        {
            ab.Property(a => a.Street)
              .HasColumnName("ShippingStreet")
              .HasMaxLength(200)
              .IsRequired();

            ab.Property(a => a.City)
              .HasColumnName("ShippingCity")
              .HasMaxLength(100)
              .IsRequired();

            ab.Property(a => a.State)
              .HasColumnName("ShippingState")
              .HasMaxLength(100)
              .IsRequired();

            ab.Property(a => a.ZipCode)
              .HasColumnName("ShippingZipCode")
              .HasMaxLength(20)
              .IsRequired();

            ab.Property(a => a.Country)
              .HasColumnName("ShippingCountry")
              .HasMaxLength(100)
              .IsRequired();
        });

        // Enum configuration
        builder.Property(o => o.Status)
               .HasConversion<string>()
               .HasMaxLength(50)
               .IsRequired();

        // Other properties
        builder.Property(o => o.OrderDate)
               .IsRequired();

        // Navigation properties
        builder.HasMany<OrderItem>()
               .WithOne()
               .HasForeignKey("OrderId")
               .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => o.OrderDate)
               .HasDatabaseName("IX_Orders_OrderDate");

        builder.HasIndex("CustomerId")
               .HasDatabaseName("IX_Orders_CustomerId");
    }
}

/// <summary>
/// Configuration for OrderItem entity
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(oi => oi.Id);

        // Value object configurations
        builder.OwnsOne(oi => oi.ProductId, pb =>
        {
            pb.Property(p => p.Value)
              .HasColumnName("ProductId")
              .IsRequired();
        });

        builder.OwnsOne(oi => oi.UnitPrice, mb =>
        {
            mb.Property(m => m.Amount)
              .HasColumnName("UnitPrice")
              .HasColumnType("decimal(18,2)")
              .IsRequired();

            mb.Property(m => m.Currency)
              .HasColumnName("Currency")
              .HasMaxLength(3)
              .IsRequired();
        });

        builder.OwnsOne(oi => oi.TotalPrice, mb =>
        {
            mb.Property(m => m.Amount)
              .HasColumnName("TotalPrice")
              .HasColumnType("decimal(18,2)")
              .IsRequired();

            mb.Property(m => m.Currency)
              .HasColumnName("TotalPriceCurrency")
              .HasMaxLength(3)
              .IsRequired();
        });

        builder.Property(oi => oi.Quantity)
               .IsRequired();

        // Indexes
        builder.HasIndex("ProductId")
               .HasDatabaseName("IX_OrderItems_ProductId");
    }
}
