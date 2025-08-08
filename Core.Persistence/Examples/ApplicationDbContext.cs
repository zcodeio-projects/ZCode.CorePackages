using Microsoft.EntityFrameworkCore;
using ZCode.Core.Domain.Examples.OrderAggregate;
using ZCode.Core.Persistence.Contexts;

namespace ZCode.Core.Persistence.Examples;

/// <summary>
/// Example application DbContext that inherits from BaseDbContext
/// All aggregate roots are automatically configured with optimistic concurrency
/// </summary>
public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets for aggregates
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    // No need to override OnModelCreating - base class handles everything automatically!
    // But you can still override if you need custom configurations:
    
    /*
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // This is important!
        
        // Add any custom configurations here if needed
        // All aggregate roots are already configured automatically
    }
    */
}
