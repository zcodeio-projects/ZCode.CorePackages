# ZCode.CorePackages - Comprehensive Usage Examples

Bu sənəd ZCode.CorePackages library-lərinin bütün xüsusiyyətlərinin istifadəsini ətraflı şəkildə göstərir.

## 📋 Table of Contents

### 🏗️ [Domain Layer Examples](#domain-layer-examples)
- [Creating Entities with Timed Events](#creating-entities-with-timed-events)
- [Value Objects](#value-objects)
- [Domain Events](#domain-events)
- [Business Rules](#business-rules)
- [Entity Builders](#entity-builders)

### 🎯 [Domain-Driven Design (DDD) Examples](#domain-driven-design-ddd-examples)
- [Aggregate Root Implementation](#aggregate-root-implementation)
- [Value Objects in DDD](#value-objects-in-ddd)
- [Domain Services](#domain-services)
- [Specification Pattern](#specification-pattern)
- [Aggregate Repository](#aggregate-repository)
- [Complete Order Aggregate Example](#complete-order-aggregate-example)
- [DDD Best Practices](#ddd-best-practices)

### 🎯 [Application Layer Examples](#application-layer-examples)
- [CQRS with MediatR](#cqrs-with-mediatr)
- [Validation Pipeline](#validation-pipeline)
- [Caching Pipeline](#caching-pipeline)
- [Transaction Pipeline](#transaction-pipeline)
- [Performance Monitoring](#performance-monitoring)

### 🗄️ [Persistence Layer Examples](#persistence-layer-examples)
- [Repository Pattern](#repository-pattern)
- [Unit of Work](#unit-of-work)
- [Database Context](#database-context)
- [Interceptors](#interceptors)
- [Migrations](#migrations)

### 🔐 [Security Examples](#security-examples)
- [JWT Authentication Setup](#jwt-authentication-setup)
- [Multi-Factor Authentication (MFA)](#multi-factor-authentication-mfa)
  - [Email Authenticator](#email-authenticator)
  - [OTP Authenticator (TOTP)](#otp-authenticator-totp)
  - [SMS Authenticator](#sms-authenticator)
  - [Complete MFA Controller Example](#complete-mfa-controller-example)
- [Authorization Attributes](#authorization-attributes)
  - [Role-Based Authorization](#role-based-authorization)
  - [Permission-Based Authorization](#permission-based-authorization)
- [Data Encryption](#data-encryption)
  - [Encryption Service Usage](#encryption-service-usage)
  - [Encryption for Configuration Values](#encryption-for-configuration-values)
- [Password Hashing](#password-hashing)

### 🗺️ [Mapping Examples](#mapping-examples)
- [AutoMapper with IMapFrom and IMapTo Patterns](#automapper-with-imapfrom-and-imapto-patterns)
  - [Service Registration](#service-registration)
  - [IMapFrom Pattern - Entity to DTO](#imapfrom-pattern---entity-to-dto)
  - [IMapTo Pattern - Command to Entity](#imapto-pattern---command-to-entity)
  - [Using IMapperService in Application Layer](#using-imapperservice-in-application-layer)
  - [Complex Mapping with Nested Objects](#complex-mapping-with-nested-objects)
  - [Conditional Mapping](#conditional-mapping)
- [Mapster Alternative Mapping](#mapster-alternative-mapping)
  - [Service Registration for Mapster](#service-registration-for-mapster)
  - [IMapsterFrom Pattern - Entity to DTO](#imapsterfrom-pattern---entity-to-dto)
  - [IMapsterTo Pattern - Command to Entity](#imapsterto-pattern---command-to-entity)
  - [Performance Comparison - AutoMapper vs Mapster](#performance-comparison---automapper-vs-mapster)
  - [Multiple Mappers Registration (Recommended)](#multiple-mappers-registration-recommended)
  - [Advanced Mapster Configuration](#advanced-mapster-configuration)

### 🧪 [Testing Examples](#testing-examples)
- [Security Component Testing](#security-component-testing)
  - [JWT Service Testing](#jwt-service-testing)
  - [Hashing Service Testing](#hashing-service-testing)
  - [OTP Authenticator Testing](#otp-authenticator-testing)
  - [Email Authenticator Testing](#email-authenticator-testing)
  - [SMS Authenticator Testing](#sms-authenticator-testing)
  - [Integration Testing with In-Memory Database](#integration-testing-with-in-memory-database)
- [Unit Testing with Test Builders](#unit-testing-with-test-builders)
- [In-Memory Database Testing](#in-memory-database-testing)
- [Integration Testing](#integration-testing)

### 📝 [Logging Examples](#logging-examples)
- [Serilog Configuration](#serilog-configuration)
- [Structured Logging](#structured-logging)
- [Performance Logging](#performance-logging)

### ⚙️ [Background Jobs Examples](#background-jobs-examples)
- [Hangfire Setup](#hangfire-setup)
- [Job Scheduling](#job-scheduling)
- [Recurring Jobs](#recurring-jobs)

### ❌ [Exception Handling Examples](#exception-handling-examples)
- [Global Exception Middleware](#global-exception-middleware)
- [Custom Exceptions](#custom-exceptions)
- [Validation Exceptions](#validation-exceptions)
- [Authorization Exceptions](#authorization-exceptions)

### ⚙️ [Configuration Examples](#configuration-examples)
- [Environment-based Configuration](#environment-based-configuration)
- [Security Configuration](#security-configuration)
- [Database Configuration](#database-configuration)

### 🚀 [Complete Project Setup](#complete-project-setup)
- [Program.cs Configuration](#programcs-configuration)
- [Service Registration](#service-registration-1)
- [Middleware Pipeline](#middleware-pipeline)
- [Best Practices](#best-practices)

## Domain Layer Examples

### Creating Entities with Timed Events
```csharp
public class User : AuditableEntity<Guid>
{
    public Email Email { get; private set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    private User() { } // EF Core

    public User(Email email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;

        // Pre-save event - will be published before SaveChanges
        AddDomainEvent(new UserCreatedEvent(Id, email.Value));

        // Post-save event - will be published after SaveChanges
        AddDomainEvent(new UserPersistedEvent(Id, email.Value));
    }
}
```

### Value Objects
```csharp
var email = Email.Create("user@example.com");
var user = new User(email, "John", "Doe");
```

## Domain-Driven Design (DDD) Examples

### Aggregate Root Implementation

#### 1. Creating an Aggregate Root
```csharp
public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _orderItems = new();

    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    public DateTime OrderDate { get; private set; }
    public Address ShippingAddress { get; private set; }

    public IReadOnlyCollection<OrderItem> OrderItems => _orderItems.AsReadOnly();

    public Order(CustomerId customerId, Address shippingAddress) : base(Guid.NewGuid())
    {
        CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        Status = OrderStatus.Pending;
        OrderDate = DateTime.UtcNow;
        TotalAmount = Money.Zero("USD");

        // Domain event
        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId.Value, OrderDate));
    }

    public void AddOrderItem(ProductId productId, Money unitPrice, int quantity)
    {
        // Business rule enforcement
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

    public void ConfirmOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");

        if (!_orderItems.Any())
            throw new InvalidOperationException("Cannot confirm order without items");

        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, TotalAmount.Amount));
    }

    // Aggregate validation
    public override bool IsValid()
    {
        return CustomerId != null &&
               ShippingAddress != null &&
               TotalAmount != null &&
               TotalAmount.Amount >= 0;
    }

    private void RecalculateTotal()
    {
        var total = _orderItems.Sum(item => item.TotalPrice.Amount);
        TotalAmount = new Money(total, TotalAmount.Currency);
    }
}
```

### Value Objects in DDD

#### 1. Money Value Object
```csharp
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

    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(Amount + other.Amount, Currency);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

#### 2. Address Value Object
```csharp
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
```

### Domain Services

#### 1. Order Domain Service
```csharp
public class OrderDomainService : IDomainService
{
    public bool CanOrderBeShipped(Order order, DateTime currentDate)
    {
        // Business rule: Orders can only be shipped on weekdays
        if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
            return false;

        if (order.Status != OrderStatus.Confirmed)
            return false;

        if (!order.OrderItems.Any())
            return false;

        return true;
    }

    public decimal CalculateShippingCost(Order order, Address destinationAddress)
    {
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
}
```

### Specification Pattern

#### 1. Order Specifications
```csharp
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

public class OrdersWithMinimumAmountSpecification : BaseSpecification<Order>
{
    public OrdersWithMinimumAmountSpecification(decimal minimumAmount, string currency)
        : base(order => order.TotalAmount.Amount >= minimumAmount && order.TotalAmount.Currency == currency)
    {
        AddInclude(order => order.OrderItems);
        AddOrderByDescending(order => order.TotalAmount.Amount);
    }
}
```

### Aggregate Repository

#### 1. Repository Registration
```csharp
// Program.cs
builder.Services.AddPersistenceServices<ApplicationDbContext>();
builder.Services.AddDddRepositories<ApplicationDbContext>();

// Or register specific repositories
builder.Services.AddScoped<IAggregateSpecificationRepository<Order, Guid>, OrderRepository>();
```

#### 2. Custom Aggregate Repository
```csharp
public class OrderRepository : EfAggregateSpecificationRepositoryBase<Order, Guid, ApplicationDbContext>
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Custom methods specific to Order aggregate
    public async Task<List<Order>> GetOrdersReadyToShipAsync()
    {
        var specification = new OrdersReadyToShipSpecification();
        return (await GetListAsync(specification)).ToList();
    }

    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
    {
        var specification = new OrdersByIdWithItemsSpecification(orderId);
        return await GetAsync(specification);
    }

    // Optimistic concurrency example
    public async Task<Order> UpdateOrderWithConcurrencyCheckAsync(Order order, int expectedVersion)
    {
        return await SaveAsync(order, expectedVersion);
    }
}
```

#### 3. Using Specifications in Repository
```csharp
public class OrderService
{
    private readonly IAggregateSpecificationRepository<Order, Guid> _orderRepository;

    public async Task<List<Order>> GetCustomerOrdersAsync(Guid customerId)
    {
        var specification = new OrdersByCustomerSpecification(customerId);
        return await _orderRepository.GetListAsync(specification);
    }

    public async Task<List<Order>> GetPendingOrdersAsync()
    {
        var specification = new OrdersByStatusSpecification(OrderStatus.Pending);
        return await _orderRepository.GetListAsync(specification);
    }

    public async Task<List<Order>> GetHighValueOrdersAsync(decimal minimumAmount)
    {
        var specification = new OrdersWithMinimumAmountSpecification(minimumAmount, "USD");
        return await _orderRepository.GetListAsync(specification);
    }

    // Combining specifications
    public async Task<List<Order>> GetCustomerPendingOrdersAsync(Guid customerId)
    {
        var customerSpec = new OrdersByCustomerSpecification(customerId);
        var pendingSpec = new OrdersByStatusSpecification(OrderStatus.Pending);
        var combinedSpec = customerSpec.And(pendingSpec);

        return await _orderRepository.GetListAsync(combinedSpec);
    }
}
```

### DDD Best Practices

#### 1. Automatic Database-Specific Concurrency Configuration
```csharp
// No need to add concurrency properties to your aggregates!
// BaseDbContext automatically configures database-specific concurrency control

public class Order : AggregateRoot<Guid>
{
    // No concurrency property needed! ✅
    // Configured automatically based on database provider:
    // - PostgreSQL: xmin system column (shadow property)
    // - SQL Server: rowversion/timestamp (shadow property)
    // - MySQL: timestamp column (shadow property)
    // - Others: Version integer property (fallback)

    public CustomerId CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount { get; private set; }
    // ... other properties
}

public class ApplicationDbContext : BaseDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Customer> Customers { get; set; }

    // All aggregates automatically get database-specific concurrency:
    // 🐘 PostgreSQL: xmin system column (uint)
    // 🏢 SQL Server: rowversion/timestamp (byte[])
    // 🐬 MySQL: timestamp column (DateTime)
    // 📦 SQLite/Others: Version integer (int)
}
```

#### 2. Custom Aggregate Configuration (Optional)
```csharp
// If you need custom configuration, inherit from AggregateRootConfiguration
public class OrderConfiguration : AggregateRootConfiguration<Order, Guid>
{
    protected override void ConfigureAggregate(EntityTypeBuilder<Order> builder)
    {
        // RowVersion property is already configured by base class!
        // builder.Property(x => x.RowVersion).IsRowVersion(); // ✅ Already done!

        // Configure only your specific properties
        builder.ToTable("Orders");

        builder.OwnsOne(o => o.TotalAmount, mb =>
        {
            mb.Property(m => m.Amount)
              .HasColumnName("TotalAmount")
              .HasColumnType("decimal(18,2)");

            mb.Property(m => m.Currency)
              .HasColumnName("Currency")
              .HasMaxLength(3);
        });

        // Navigation properties
        builder.HasMany<OrderItem>()
               .WithOne()
               .HasForeignKey("OrderId");
    }
}
```

#### 3. Database-Specific Concurrency Usage in Service Layer
```csharp
public class OrderService
{
    private readonly IAggregateRepository<Order, Guid> _orderRepository;
    private readonly DbContext _context;

    public async Task UpdateOrderAsync(Guid orderId, UpdateOrderCommand command)
    {
        // 1. Get order
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) throw new NotFoundException("Order not found");

        // 2. Get current concurrency token (database-specific type)
        var currentToken = _orderRepository.GetConcurrencyToken(order);
        // PostgreSQL: uint (xmin)
        // SQL Server: byte[] (rowversion)
        // MySQL: DateTime (timestamp)
        // Others: int (version)

        // 3. Apply business logic
        order.UpdateShippingAddress(command.NewAddress);
        order.AddOrderItem(command.ProductId, command.UnitPrice, command.Quantity);

        // 4. Save with optimistic concurrency check
        try
        {
            await _orderRepository.SaveAsync(order, currentToken);
        }
        catch (ConcurrencyException)
        {
            throw new BusinessException("Order was modified by another user. Please refresh and try again.");
        }
    }

    // Get order with concurrency token for client-side tracking
    public async Task<OrderWithConcurrencyDto> GetOrderWithConcurrencyTokenAsync(Guid orderId)
    {
        var order = await _orderRepository.GetByIdAsync(orderId);
        if (order == null) return null;

        var concurrencyToken = _orderRepository.GetConcurrencyToken(order);

        return new OrderWithConcurrencyDto
        {
            Id = order.Id,
            Status = order.Status.ToString(),
            TotalAmount = order.TotalAmount.Amount,
            ConcurrencyToken = concurrencyToken // Database-specific token
        };
    }
}

// Database-specific concurrency token examples:
public class OrderWithConcurrencyDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public object ConcurrencyToken { get; set; } = null!; // Can be uint, byte[], DateTime, or int
}
```

#### 4. Database Provider Examples
```csharp
// PostgreSQL Configuration
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
// Result: xmin system column (uint) for concurrency

// SQL Server Configuration
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// Result: rowversion/timestamp (byte[]) for concurrency

// MySQL Configuration
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
// Result: timestamp column (DateTime) for concurrency

// SQLite Configuration (fallback)
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
// Result: Version integer (int) for concurrency
}
```

#### 4. Global Configuration Benefits
```csharp
// ✅ Automatic optimistic concurrency with SQL Server RowVersion
// ✅ No repetitive RowVersion property configuration
// ✅ Database-managed concurrency tokens (more efficient than manual versioning)
// ✅ Consistent audit field configuration
// ✅ Automatic soft delete filters
// ✅ Domain events automatically ignored

// Before (manual configuration for each aggregate):
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.Property(x => x.RowVersion).IsRowVersion(); // Repetitive!
        builder.Ignore(x => x.DomainEvents);                // Repetitive!
        // ... other configurations
    }
}

// After (automatic configuration):
public class OrderConfiguration : AggregateRootConfiguration<Order, Guid>
{
    protected override void ConfigureAggregate(EntityTypeBuilder<Order> builder)
    {
        // RowVersion and DomainEvents already configured!
        // Focus only on business-specific configurations
    }
}
```

#### 4. Service Registration
```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// All aggregate configurations are applied automatically!
```

### Specifications
```csharp
public class ActiveUserSpecification : Specification<User>
{
    public override Expression<Func<User, bool>> ToExpression()
    {
        return user => user.DeletedDate == null && user.IsActive;
    }
}

// Usage
var activeUsers = await repository.GetListBySpecificationAsync(
    new ActiveUserSpecification()
);
```

## Application Layer Examples

### CQRS Commands/Queries
```csharp
public class CreateUserCommand : IRequest<Result<UserDto>>, ITransactionalRequest
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IAsyncRepository<User, Guid> _userRepository;
    
    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = new User(email, request.FirstName, request.LastName);
        
        await _userRepository.AddAsync(user, cancellationToken);
        
        return Result.Success(new UserDto { Id = user.Id, Email = user.Email });
    }
}
```

### Caching
```csharp
public class GetUserQuery : IRequest<UserDto>, ICachableRequest
{
    public Guid Id { get; set; }
    
    public bool BypassCache { get; set; }
    public string CacheKey => $"User-{Id}";
    public string? CacheGroupKey => "Users";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);
}
```

## Persistence Layer Examples

### Repository Usage
```csharp
// Get with includes
var user = await userRepository.GetAsync(
    predicate: u => u.Id == userId,
    include: u => u.Include(x => x.Orders),
    enableTracking: false
);

// Dynamic queries
var dynamicQuery = new DynamicQuery
{
    Filter = new Filter { Field = "FirstName", Operator = "contains", Value = "John" },
    Sort = new[] { new Sort { Field = "CreatedDate", Dir = "desc" } }
};

var users = await userRepository.GetListByDynamicAsync(dynamicQuery);

// Pagination
var pagedUsers = await userRepository.GetListAsync(
    index: 0, 
    size: 10,
    orderBy: q => q.OrderBy(u => u.FirstName)
);
```

### Unit of Work
```csharp
public class UserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncRepository<User, Guid> _userRepository;
    
    public async Task TransferUsersAsync(List<User> users)
    {
        await _unitOfWork.BeginTransactionAsync();
        
        try
        {
            foreach (var user in users)
            {
                await _userRepository.UpdateAsync(user);
            }
            
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
```

## Configuration Examples

### Startup Configuration
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplicationServices(Assembly.GetExecutingAssembly());
builder.Services.AddPersistenceServices<ApplicationDbContext>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .AddInterceptors(
               serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptors<Guid>>(),
               serviceProvider.GetRequiredService<DomainEventsInterceptor>()
           ));

var app = builder.Build();

// Configure middleware
app.ConfigureCustomExceptionMiddleware();
```

### DbContext Configuration
```csharp
public class ApplicationDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register all entities
        modelBuilder.RegisterAllEntities<IEntity<Guid>>(Assembly.GetExecutingAssembly());
        
        // Apply soft delete filter
        modelBuilder.ApplySoftDeleteQueryFilter();

        base.OnModelCreating(modelBuilder);
    }
}
```

## Event Timing Examples

### Pre-Save vs Post-Save Events
```csharp
// Pre-save event - published before SaveChanges
public class UserValidationEvent : DomainEvent, IPreSaveDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserValidationEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}

// Post-save event - published after SaveChanges
public class UserNotificationEvent : DomainEvent, IPostSaveDomainEvent
{
    public Guid UserId { get; }
    public string Email { get; }

    public UserNotificationEvent(Guid userId, string email)
    {
        UserId = userId;
        Email = email;
    }
}

// Event handler that can trigger nested events
public class UserValidationEventHandler : INotificationHandler<UserValidationEvent>
{
    private readonly IDomainEventPublisher _eventPublisher;

    public async Task Handle(UserValidationEvent notification, CancellationToken cancellationToken)
    {
        // Validate user
        if (await IsEmailDuplicate(notification.Email))
        {
            // Queue another event to be processed after current event
            await _eventPublisher.QueueEventAsync(
                new UserEmailDuplicateEvent(notification.UserId, notification.Email),
                cancellationToken);
        }
    }
}
```

### Nested Event Publishing
```csharp
public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IDomainEventPublisher _eventPublisher;

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Process order
        await ProcessOrder(notification.OrderId);

        // Trigger nested events
        await _eventPublisher.QueueEventAsync(
            new InventoryUpdatedEvent(notification.ProductId, notification.Quantity),
            cancellationToken);

        await _eventPublisher.QueueEventAsync(
            new CustomerNotificationEvent(notification.CustomerId, "Order created"),
            cancellationToken);
    }
}
```

## Security Examples

### JWT Authentication Setup

#### 1. Configuration (appsettings.json)
```json
{
  "TokenOptions": {
    "SecurityKey": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 60,
    "RefreshTokenTTL": 7
  },
  "EncryptionOptions": {
    "Key": "your-32-character-encryption-key",
    "IV": "your-16-char-iv"
  }
}
```

#### 2. Service Registration
```csharp
// Program.cs
using ZCode.Core.Security.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add security services
builder.Services.AddSecurityServices<Guid, Guid>(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthenticatorServices();
builder.Services.AddHashingService();
builder.Services.AddEncryptionService(builder.Configuration);

var app = builder.Build();

// Add authentication middleware
app.UseAuthentication();
app.UseAuthorization();
```

#### 3. JWT Service Usage
```csharp
public class AuthService
{
    private readonly IJwtService _jwtService;
    private readonly IHashingService _hashingService;
    private readonly ICurrentUserService _currentUserService;

    public AuthService(IJwtService jwtService, IHashingService hashingService, ICurrentUserService currentUserService)
    {
        _jwtService = jwtService;
        _hashingService = hashingService;
        _currentUserService = currentUserService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Validate user credentials
        var user = await GetUserByEmailAsync(request.Email);
        if (user == null || !_hashingService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new AuthorizationException("Invalid credentials");
        }

        // Create claims
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        // Generate tokens
        var accessToken = _jwtService.GenerateToken(claims);
        var refreshToken = _jwtService.GenerateRefreshToken();

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = _jwtService.GetTokenExpiration(accessToken)
        };
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // Hash password
        var passwordHash = _hashingService.HashPassword(request.Password);

        // Create user
        var user = new User(
            Email.Create(request.Email),
            request.FirstName,
            request.LastName
        );
        user.SetPassword(passwordHash);

        await _userRepository.AddAsync(user);

        return new RegisterResponse { UserId = user.Id };
    }
}
```

#### 4. Current User Service Usage
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public ProfileController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = _currentUserService.UserId;
        var userEmail = _currentUserService.Email;
        var userRoles = _currentUserService.Roles;

        if (!_currentUserService.IsAuthenticated)
        {
            return Unauthorized();
        }

        if (_currentUserService.IsInRole("Admin"))
        {
            // Admin specific logic
        }

        if (_currentUserService.HasPermission("read:profile"))
        {
            // Permission specific logic
        }

        return Ok(new { UserId = userId, Email = userEmail, Roles = userRoles });
    }
}
```

### Password Hashing
```csharp
public class UserService
{
    private readonly IHashingService _hashingService;

    public UserService(IHashingService hashingService)
    {
        _hashingService = hashingService;
    }

    public async Task CreateUserAsync(string email, string password)
    {
        // Hash password before storing
        var hashedPassword = _hashingService.HashPassword(password);

        var user = new User(Email.Create(email), hashedPassword);
        await _userRepository.AddAsync(user);
    }

    public async Task<bool> ValidatePasswordAsync(string email, string password)
    {
        var user = await _userRepository.GetAsync(u => u.Email.Value == email);
        if (user == null) return false;

        return _hashingService.VerifyPassword(password, user.PasswordHash);
    }
}
```

### Multi-Factor Authentication (MFA)

#### Email Authenticator
```csharp
public class EmailAuthService
{
    private readonly IEmailAuthenticatorHelper _emailAuthHelper;
    private readonly IRepository<EmailAuthenticator<Guid>, Guid> _emailAuthRepository;
    private readonly IEmailService _emailService;

    public EmailAuthService(
        IEmailAuthenticatorHelper emailAuthHelper,
        IRepository<EmailAuthenticator<Guid>, Guid> emailAuthRepository,
        IEmailService emailService)
    {
        _emailAuthHelper = emailAuthHelper;
        _emailAuthRepository = emailAuthRepository;
        _emailService = emailService;
    }

    public async Task<string> SendVerificationEmailAsync(Guid userId, string email)
    {
        // Generate activation code
        var activationCode = await _emailAuthHelper.CreateEmailActivationCode();
        var activationKey = await _emailAuthHelper.CreateEmailActivationKey();

        // Create or update email authenticator
        var emailAuth = await _emailAuthRepository.GetAsync(e => e.UserId.Equals(userId));
        if (emailAuth == null)
        {
            emailAuth = new EmailAuthenticator<Guid>(userId, false);
            await _emailAuthRepository.AddAsync(emailAuth);
        }

        emailAuth.ActivationKey = activationKey;
        emailAuth.IsVerified = false;
        await _emailAuthRepository.UpdateAsync(emailAuth);

        // Send email with activation code
        await _emailService.SendAsync(email, "Email Verification",
            $"Your verification code is: {activationCode}");

        return activationKey;
    }

    public async Task<bool> VerifyEmailAsync(Guid userId, string activationKey)
    {
        var emailAuth = await _emailAuthRepository.GetAsync(e =>
            e.UserId.Equals(userId) && e.ActivationKey == activationKey);

        if (emailAuth == null) return false;

        emailAuth.IsVerified = true;
        emailAuth.ActivationKey = null;
        await _emailAuthRepository.UpdateAsync(emailAuth);

        return true;
    }
}
```

#### OTP Authenticator (TOTP)
```csharp
public class OtpAuthService
{
    private readonly IOtpAuthenticatorHelper _otpHelper;
    private readonly IRepository<OtpAuthenticator<Guid>, Guid> _otpRepository;

    public OtpAuthService(
        IOtpAuthenticatorHelper otpHelper,
        IRepository<OtpAuthenticator<Guid>, Guid> otpRepository)
    {
        _otpHelper = otpHelper;
        _otpRepository = otpRepository;
    }

    public async Task<OtpSetupResponse> SetupOtpAsync(Guid userId, string accountName, string issuer)
    {
        // Generate secret key
        var secretKey = await _otpHelper.GenerateSecretKey();
        var secretKeyString = await _otpHelper.ConvertSecretKeyToString(secretKey);
        var qrCodeUri = await _otpHelper.GenerateQrCodeUri(secretKey, accountName, issuer);

        // Create OTP authenticator
        var otpAuth = new OtpAuthenticator<Guid>(userId, secretKey, false);
        await _otpRepository.AddAsync(otpAuth);

        return new OtpSetupResponse
        {
            SecretKey = secretKeyString,
            QrCodeUri = qrCodeUri,
            ManualEntryKey = secretKeyString
        };
    }

    public async Task<bool> VerifyOtpAsync(Guid userId, string code)
    {
        var otpAuth = await _otpRepository.GetAsync(o => o.UserId.Equals(userId));
        if (otpAuth == null) return false;

        var isValid = await _otpHelper.VerifyCode(otpAuth.SecretKey, code);

        if (isValid && !otpAuth.IsVerified)
        {
            otpAuth.IsVerified = true;
            await _otpRepository.UpdateAsync(otpAuth);
        }

        return isValid;
    }
}

public class OtpSetupResponse
{
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUri { get; set; } = string.Empty;
    public string ManualEntryKey { get; set; } = string.Empty;
}
```

#### SMS Authenticator
```csharp
public class SmsAuthService
{
    private readonly ISmsAuthenticatorHelper _smsHelper;
    private readonly IRepository<SmsAuthenticator<Guid>, Guid> _smsRepository;
    private readonly ISmsService _smsService;

    public SmsAuthService(
        ISmsAuthenticatorHelper smsHelper,
        IRepository<SmsAuthenticator<Guid>, Guid> smsRepository,
        ISmsService smsService)
    {
        _smsHelper = smsHelper;
        _smsRepository = smsRepository;
        _smsService = smsService;
    }

    public async Task SendVerificationSmsAsync(Guid userId, string phoneNumber)
    {
        // Generate SMS code
        var activationCode = await _smsHelper.CreateSmsActivationCode();

        // Create or update SMS authenticator
        var smsAuth = await _smsRepository.GetAsync(s => s.UserId.Equals(userId));
        if (smsAuth == null)
        {
            smsAuth = new SmsAuthenticator<Guid>(userId, phoneNumber, false);
            await _smsRepository.AddAsync(smsAuth);
        }

        smsAuth.SetActivationCode(activationCode);
        await _smsRepository.UpdateAsync(smsAuth);

        // Send SMS
        await _smsService.SendAsync(phoneNumber, $"Your verification code is: {activationCode}");
    }

    public async Task<bool> VerifySmsAsync(Guid userId, string code)
    {
        var smsAuth = await _smsRepository.GetAsync(s => s.UserId.Equals(userId));
        if (smsAuth?.ActivationCode == null) return false;

        // Check if code is expired
        var isExpired = await _smsHelper.IsCodeExpired(smsAuth.CodeCreatedAt ?? DateTime.UtcNow);
        if (isExpired) return false;

        // Verify code
        var isValid = await _smsHelper.VerifyCode(smsAuth.ActivationCode, code);

        if (isValid)
        {
            smsAuth.VerifyCode();
            await _smsRepository.UpdateAsync(smsAuth);
        }

        return isValid;
    }
}
```

#### Complete MFA Controller Example
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly EmailAuthService _emailAuthService;
    private readonly OtpAuthService _otpAuthService;
    private readonly SmsAuthService _smsAuthService;
    private readonly ICurrentUserService _currentUserService;

    public MfaController(
        EmailAuthService emailAuthService,
        OtpAuthService otpAuthService,
        SmsAuthService smsAuthService,
        ICurrentUserService currentUserService)
    {
        _emailAuthService = emailAuthService;
        _otpAuthService = otpAuthService;
        _smsAuthService = smsAuthService;
        _currentUserService = currentUserService;
    }

    [HttpPost("email/send")]
    public async Task<IActionResult> SendEmailVerification([FromBody] SendEmailVerificationRequest request)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);
        var activationKey = await _emailAuthService.SendVerificationEmailAsync(userId, request.Email);

        return Ok(new { Message = "Verification email sent", ActivationKey = activationKey });
    }

    [HttpPost("email/verify")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);
        var isVerified = await _emailAuthService.VerifyEmailAsync(userId, request.ActivationKey);

        return Ok(new { IsVerified = isVerified });
    }

    [HttpPost("otp/setup")]
    public async Task<IActionResult> SetupOtp([FromBody] SetupOtpRequest request)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);
        var setup = await _otpAuthService.SetupOtpAsync(userId, request.AccountName, request.Issuer);

        return Ok(setup);
    }

    [HttpPost("otp/verify")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);
        var isValid = await _otpAuthService.VerifyOtpAsync(userId, request.Code);

        return Ok(new { IsValid = isValid });
    }

    [HttpPost("sms/send")]
    public async Task<IActionResult> SendSmsVerification([FromBody] SendSmsVerificationRequest request)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);
        await _smsAuthService.SendVerificationSmsAsync(userId, request.PhoneNumber);

        return Ok(new { Message = "Verification SMS sent" });
    }

    [HttpPost("sms/verify")]
    public async Task<IActionResult> VerifySms([FromBody] VerifySmsRequest request)
    {
        var userId = Guid.Parse(_currentUserService.UserId!);
        var isValid = await _smsAuthService.VerifySmsAsync(userId, request.Code);

        return Ok(new { IsValid = isValid });
    }
}

// Request/Response DTOs
public record SendEmailVerificationRequest(string Email);
public record VerifyEmailRequest(string ActivationKey);
public record SetupOtpRequest(string AccountName, string Issuer);
public record VerifyOtpRequest(string Code);
public record SendSmsVerificationRequest(string PhoneNumber);
public record VerifySmsRequest(string Code);
```

### Authorization Attributes

#### Role-Based Authorization
```csharp
[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    [HttpGet("users")]
    [RequireRole("Admin", "SuperAdmin")]
    public async Task<IActionResult> GetAllUsers()
    {
        // Only users with Admin or SuperAdmin role can access this
        return Ok();
    }

    [HttpDelete("users/{id}")]
    [RequireRole("SuperAdmin")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        // Only SuperAdmin can delete users
        return Ok();
    }
}
```

#### Permission-Based Authorization
```csharp
[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    [HttpGet]
    [RequirePermission("documents:read")]
    public async Task<IActionResult> GetDocuments()
    {
        // Only users with documents:read permission can access
        return Ok();
    }

    [HttpPost]
    [RequirePermission("documents:create")]
    public async Task<IActionResult> CreateDocument([FromBody] CreateDocumentRequest request)
    {
        // Only users with documents:create permission can create
        return Ok();
    }

    [HttpDelete("{id}")]
    [RequirePermission("documents:delete", "admin:all")]
    public async Task<IActionResult> DeleteDocument(Guid id)
    {
        // Users need either documents:delete OR admin:all permission
        return Ok();
    }
}
```

### Data Encryption

#### Encryption Service Usage
```csharp
public class UserService
{
    private readonly IEncryptionService _encryptionService;
    private readonly IRepository<User, Guid> _userRepository;

    public UserService(IEncryptionService encryptionService, IRepository<User, Guid> userRepository)
    {
        _encryptionService = encryptionService;
        _userRepository = userRepository;
    }

    public async Task CreateUserAsync(CreateUserRequest request)
    {
        // Encrypt sensitive data before storing
        var encryptedSsn = _encryptionService.Encrypt(request.SocialSecurityNumber);
        var encryptedPhone = _encryptionService.Encrypt(request.PhoneNumber);

        var user = new User(
            Email.Create(request.Email),
            request.FirstName,
            request.LastName,
            encryptedSsn,
            encryptedPhone
        );

        await _userRepository.AddAsync(user);
    }

    public async Task<UserDetailsResponse> GetUserDetailsAsync(Guid userId)
    {
        var user = await _userRepository.GetAsync(u => u.Id == userId);
        if (user == null) return null;

        // Decrypt sensitive data when retrieving
        var decryptedSsn = _encryptionService.Decrypt(user.EncryptedSsn);
        var decryptedPhone = _encryptionService.Decrypt(user.EncryptedPhone);

        return new UserDetailsResponse
        {
            Id = user.Id,
            Email = user.Email.Value,
            FirstName = user.FirstName,
            LastName = user.LastName,
            SocialSecurityNumber = decryptedSsn,
            PhoneNumber = decryptedPhone
        };
    }
}
```

#### Encryption for Configuration Values
```csharp
public class ConfigurationService
{
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;

    public ConfigurationService(IEncryptionService encryptionService, IConfiguration configuration)
    {
        _encryptionService = encryptionService;
        _configuration = configuration;
    }

    public string GetDecryptedConnectionString(string name)
    {
        var encryptedConnectionString = _configuration.GetConnectionString(name);
        if (string.IsNullOrEmpty(encryptedConnectionString))
            return string.Empty;

        return _encryptionService.Decrypt(encryptedConnectionString);
    }

    public string GetDecryptedApiKey(string keyName)
    {
        var encryptedApiKey = _configuration[$"ApiKeys:{keyName}"];
        if (string.IsNullOrEmpty(encryptedApiKey))
            return string.Empty;

        return _encryptionService.Decrypt(encryptedApiKey);
    }
}
```

## Mapping Examples

### AutoMapper with IMapFrom and IMapTo Patterns

#### 1. Service Registration
```csharp
// Program.cs
using ZCode.Core.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add AutoMapper services with automatic profile discovery
builder.Services.AddAutoMapperServices(typeof(Program).Assembly);

// Or with custom configuration
builder.Services.AddAutoMapperServices(cfg =>
{
    cfg.AllowNullCollections = true;
    cfg.AllowNullDestinationValues = true;
}, typeof(Program).Assembly);
```

#### 2. IMapFrom Pattern - Entity to DTO
```csharp
public class UserDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Custom mapping configuration
    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));
    }
}
```

#### 3. IMapTo Pattern - Command to Entity
```csharp
public class CreateUserCommand : IMapTo<User>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Custom mapping configuration
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => Email.Create(src.Email)))
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}
```

#### 4. Using IMapperService in Application Layer
```csharp
public class UserService
{
    private readonly IMapperService _mapper;
    private readonly IRepository<User, Guid> _userRepository;

    public UserService(IMapperService mapper, IRepository<User, Guid> userRepository)
    {
        _mapper = mapper;
        _userRepository = userRepository;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserCommand command)
    {
        // Map command to entity using IMapTo pattern
        var user = _mapper.Map<User>(command);

        var createdUser = await _userRepository.AddAsync(user);

        // Map entity to DTO using IMapFrom pattern
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetAllAsync();

        // Map collection
        return _mapper.Map<List<UserDto>>(users).ToList();
    }

    public async Task<UserDto> UpdateUserAsync(Guid id, UpdateUserCommand command)
    {
        var existingUser = await _userRepository.GetAsync(u => u.Id == id);
        if (existingUser == null)
            throw new NotFoundException("User not found");

        // Map command to existing entity (merge)
        _mapper.Map(command, existingUser);

        var updatedUser = await _userRepository.UpdateAsync(existingUser);
        return _mapper.Map<UserDto>(updatedUser);
    }
}
```

#### 5. Complex Mapping with Nested Objects
```csharp
public class UserDetailDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = null!;
    public List<OrderDto> Orders { get; set; } = new();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.Orders, opt => opt.MapFrom(src => src.Orders));
    }
}

public class AddressDto : IMapFrom<Address>
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class OrderDto : IMapFrom<Order>
{
    public Guid Id { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}
```

#### 6. Conditional Mapping
```csharp
public class UserSummaryDto : IMapFrom<User>
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserSummaryDto>()
            .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src =>
                string.IsNullOrEmpty(src.FirstName) ? src.Email.Value : $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.IsActive ? "Active" : "Inactive"))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
    }
}
```

### Mapster Alternative Mapping

#### 1. Service Registration for Mapster
```csharp
// Program.cs - Using Mapster instead of AutoMapper
using ZCode.Core.Application.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Mapster services with automatic configuration discovery
builder.Services.AddMapsterServices(typeof(Program).Assembly);
```

#### 2. IMapsterFrom Pattern - Entity to DTO
```csharp
public class UserMapsterDto : IMapsterFrom<User>
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }

    // Custom mapping configuration with Mapster
    public void Mapping(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserMapsterDto>()
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.FullName, src => $"{src.FirstName} {src.LastName}");
    }
}
```

#### 3. IMapsterTo Pattern - Command to Entity
```csharp
public class CreateUserMapsterCommand : IMapsterTo<User>
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Custom mapping configuration with Mapster
    public void Mapping(TypeAdapterConfig config)
    {
        config.NewConfig<CreateUserMapsterCommand, User>()
            .Map(dest => dest.Id, src => Guid.NewGuid())
            .Map(dest => dest.Email, src => Email.Create(src.Email))
            .Map(dest => dest.CreatedDate, src => DateTime.UtcNow);
    }
}
```

#### 4. Performance Comparison - AutoMapper vs Mapster
```csharp
public class MappingPerformanceService
{
    private readonly IMapperService _autoMapperService;
    private readonly IMapperService _mapsterService;

    public MappingPerformanceService(
        [FromKeyedServices("AutoMapper")] IMapperService autoMapperService,
        [FromKeyedServices("Mapster")] IMapperService mapsterService)
    {
        _autoMapperService = autoMapperService;
        _mapsterService = mapsterService;
    }

    public async Task<BenchmarkResult> CompareMappingPerformance(List<User> users)
    {
        var stopwatch = new Stopwatch();

        // AutoMapper performance
        stopwatch.Start();
        var autoMapperResults = _autoMapperService.Map<List<UserDto>>(users);
        stopwatch.Stop();
        var autoMapperTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Reset();

        // Mapster performance
        stopwatch.Start();
        var mapsterResults = _mapsterService.Map<List<UserMapsterDto>>(users);
        stopwatch.Stop();
        var mapsterTime = stopwatch.ElapsedMilliseconds;

        return new BenchmarkResult
        {
            AutoMapperTime = autoMapperTime,
            MapsterTime = mapsterTime,
            PerformanceGain = ((double)(autoMapperTime - mapsterTime) / autoMapperTime) * 100
        };
    }
}

public class BenchmarkResult
{
    public long AutoMapperTime { get; set; }
    public long MapsterTime { get; set; }
    public double PerformanceGain { get; set; }
}
```

#### 5. Multiple Mappers Registration (Recommended)
```csharp
// Program.cs - Register both AutoMapper and Mapster with Strategy Selector
var builder = WebApplication.CreateBuilder(args);

// Option 1: Register both mappers with strategy selector (Recommended)
builder.Services.AddBothMappingServices(typeof(Program).Assembly);

// Option 2: Manual registration
// builder.Services.AddAutoMapperServices(typeof(Program).Assembly);
// builder.Services.AddMapsterServices(typeof(Program).Assembly);
// builder.Services.AddMappingStrategySelector();

// Usage with Strategy Selector (Recommended)
public class UserService
{
    private readonly IMappingStrategySelector _mappingSelector;
    private readonly IRepository<User, Guid> _userRepository;

    public UserService(
        IMappingStrategySelector mappingSelector,
        IRepository<User, Guid> userRepository)
    {
        _mappingSelector = mappingSelector;
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetUserAsync(Guid id, MappingStrategy strategy = MappingStrategy.Mapster)
    {
        var user = await _userRepository.GetAsync(u => u.Id == id);
        var mapper = _mappingSelector.GetMapper(strategy);
        return mapper.Map<UserDto>(user);
    }

    // Or use specific mappers directly
    public async Task<UserDto> GetUserWithAutoMapper(Guid id)
    {
        var user = await _userRepository.GetAsync(u => u.Id == id);
        var mapper = _mappingSelector.GetMapper(MappingStrategy.AutoMapper);
        return mapper.Map<UserDto>(user);
    }

    public async Task<UserMapsterDto> GetUserWithMapster(Guid id)
    {
        var user = await _userRepository.GetAsync(u => u.Id == id);
        var mapper = _mappingSelector.GetMapper(MappingStrategy.Mapster);
        return mapper.Map<UserMapsterDto>(user);
    }
}
```

#### 6. Advanced Mapster Configuration
```csharp
public class AdvancedUserDto : IMapsterFrom<User>
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserStatus Status { get; set; }
    public List<string> Roles { get; set; } = new();

    public void Mapping(TypeAdapterConfig config)
    {
        config.NewConfig<User, AdvancedUserDto>()
            .Map(dest => dest.Email, src => src.Email.Value)
            .Map(dest => dest.DisplayName, src =>
                string.IsNullOrEmpty(src.FirstName) ? src.Email.Value : $"{src.FirstName} {src.LastName}")
            .Map(dest => dest.Status, src => src.IsActive ? UserStatus.Active : UserStatus.Inactive)
            .Map(dest => dest.Roles, src => src.UserRoles.Select(ur => ur.Role.Name))
            .IgnoreNullValues(true)
            .PreserveReference(true);
    }
}

public enum UserStatus
{
    Active,
    Inactive,
    Suspended
}
```

## Testing Examples

### Security Component Testing

#### 1. JWT Service Testing
```csharp
[TestFixture]
public class JwtServiceTests
{
    private JwtService<Guid, Guid> _jwtService;
    private TokenOption _tokenOptions;

    [SetUp]
    public void Setup()
    {
        _tokenOptions = new TokenOption
        {
            SecurityKey = "test-secret-key-that-is-at-least-32-characters-long",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            ExpirationMinutes = 60,
            RefreshTokenTTL = 7
        };

        var options = Options.Create(_tokenOptions);
        _jwtService = new JwtService<Guid, Guid>(options);
    }

    [Test]
    public void Should_Generate_Valid_Access_Token()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "test@example.com"),
            new(ClaimTypes.Role, "User")
        };

        // Act
        var accessToken = _jwtService.GenerateToken(claims);

        // Assert
        Assert.That(accessToken.Token, Is.Not.Null.And.Not.Empty);
        Assert.That(accessToken.ExpirationDate, Is.GreaterThan(DateTime.UtcNow));
    }

    [Test]
    public void Should_Validate_Token_Successfully()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Email, "test@example.com")
        };
        var accessToken = _jwtService.GenerateToken(claims);

        // Act
        var principal = _jwtService.ValidateToken(accessToken.Token);

        // Assert
        Assert.That(principal, Is.Not.Null);
        Assert.That(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, Is.EqualTo("123"));
        Assert.That(principal.FindFirst(ClaimTypes.Email)?.Value, Is.EqualTo("test@example.com"));
    }

    [Test]
    public void Should_Return_Null_For_Invalid_Token()
    {
        // Act
        var principal = _jwtService.ValidateToken("invalid-token");

        // Assert
        Assert.That(principal, Is.Null);
    }
}
```

#### 2. Hashing Service Testing
```csharp
[TestFixture]
public class BCryptHashingServiceTests
{
    private BCryptHashingService _hashingService;

    [SetUp]
    public void Setup()
    {
        _hashingService = new BCryptHashingService(workFactor: 4); // Lower work factor for faster tests
    }

    [Test]
    public void Should_Hash_Password_Successfully()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        var hashedPassword = _hashingService.HashPassword(password);

        // Assert
        Assert.That(hashedPassword, Is.Not.Null.And.Not.Empty);
        Assert.That(hashedPassword, Is.Not.EqualTo(password));
        Assert.That(hashedPassword.Length, Is.GreaterThan(50));
    }

    [Test]
    public void Should_Verify_Correct_Password()
    {
        // Arrange
        const string password = "TestPassword123!";
        var hashedPassword = _hashingService.HashPassword(password);

        // Act
        var isValid = _hashingService.VerifyPassword(password, hashedPassword);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void Should_Reject_Incorrect_Password()
    {
        // Arrange
        const string password = "TestPassword123!";
        const string wrongPassword = "WrongPassword123!";
        var hashedPassword = _hashingService.HashPassword(password);

        // Act
        var isValid = _hashingService.VerifyPassword(wrongPassword, hashedPassword);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void Should_Generate_Different_Hashes_For_Same_Password()
    {
        // Arrange
        const string password = "TestPassword123!";

        // Act
        var hash1 = _hashingService.HashPassword(password);
        var hash2 = _hashingService.HashPassword(password);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
        Assert.That(_hashingService.VerifyPassword(password, hash1), Is.True);
        Assert.That(_hashingService.VerifyPassword(password, hash2), Is.True);
    }
}
```

#### 3. OTP Authenticator Testing
```csharp
[TestFixture]
public class OtpNetOtpAuthenticatorHelperTests
{
    private OtpNetOtpAuthenticatorHelper _otpHelper;

    [SetUp]
    public void Setup()
    {
        _otpHelper = new OtpNetOtpAuthenticatorHelper();
    }

    [Test]
    public async Task Should_Generate_Secret_Key()
    {
        // Act
        var secretKey = await _otpHelper.GenerateSecretKey();

        // Assert
        Assert.That(secretKey, Is.Not.Null);
        Assert.That(secretKey.Length, Is.GreaterThan(0));
    }

    [Test]
    public async Task Should_Convert_Secret_Key_To_String()
    {
        // Arrange
        var secretKey = await _otpHelper.GenerateSecretKey();

        // Act
        var secretKeyString = await _otpHelper.ConvertSecretKeyToString(secretKey);

        // Assert
        Assert.That(secretKeyString, Is.Not.Null.And.Not.Empty);
        Assert.That(secretKeyString.Length, Is.GreaterThan(10));
    }

    [Test]
    public async Task Should_Generate_QR_Code_Uri()
    {
        // Arrange
        var secretKey = await _otpHelper.GenerateSecretKey();
        const string accountName = "test@example.com";
        const string issuer = "TestApp";

        // Act
        var qrCodeUri = await _otpHelper.GenerateQrCodeUri(secretKey, accountName, issuer);

        // Assert
        Assert.That(qrCodeUri, Is.Not.Null.And.Not.Empty);
        Assert.That(qrCodeUri, Does.StartWith("otpauth://totp/"));
        Assert.That(qrCodeUri, Does.Contain(accountName));
        Assert.That(qrCodeUri, Does.Contain(issuer));
    }

    [Test]
    public async Task Should_Verify_Valid_Code()
    {
        // Arrange
        var secretKey = await _otpHelper.GenerateSecretKey();
        var totp = new Totp(secretKey);
        var validCode = totp.ComputeTotp(DateTime.UtcNow);

        // Act
        var isValid = await _otpHelper.VerifyCode(secretKey, validCode);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public async Task Should_Reject_Invalid_Code()
    {
        // Arrange
        var secretKey = await _otpHelper.GenerateSecretKey();
        const string invalidCode = "123456";

        // Act
        var isValid = await _otpHelper.VerifyCode(secretKey, invalidCode);

        // Assert
        Assert.That(isValid, Is.False);
    }
}
```

#### 4. Email Authenticator Testing
```csharp
[TestFixture]
public class EmailAuthenticatorHelperTests
{
    private EmailAuthenticatorHelper _emailHelper;

    [SetUp]
    public void Setup()
    {
        _emailHelper = new EmailAuthenticatorHelper();
    }

    [Test]
    public async Task Should_Create_Email_Activation_Key()
    {
        // Act
        var activationKey = await _emailHelper.CreateEmailActivationKey();

        // Assert
        Assert.That(activationKey, Is.Not.Null.And.Not.Empty);
        Assert.That(activationKey.Length, Is.GreaterThan(50));
    }

    [Test]
    public async Task Should_Create_Email_Activation_Code()
    {
        // Act
        var activationCode = await _emailHelper.CreateEmailActivationCode();

        // Assert
        Assert.That(activationCode, Is.Not.Null.And.Not.Empty);
        Assert.That(activationCode.Length, Is.EqualTo(6));
        Assert.That(activationCode, Does.Match(@"^\d{6}$"));
    }

    [Test]
    public async Task Should_Generate_Different_Activation_Keys()
    {
        // Act
        var key1 = await _emailHelper.CreateEmailActivationKey();
        var key2 = await _emailHelper.CreateEmailActivationKey();

        // Assert
        Assert.That(key1, Is.Not.EqualTo(key2));
    }
}
```

#### 5. SMS Authenticator Testing
```csharp
[TestFixture]
public class SmsAuthenticatorHelperTests
{
    private SmsAuthenticatorHelper _smsHelper;

    [SetUp]
    public void Setup()
    {
        _smsHelper = new SmsAuthenticatorHelper();
    }

    [Test]
    public async Task Should_Create_Sms_Activation_Code()
    {
        // Act
        var activationCode = await _smsHelper.CreateSmsActivationCode();

        // Assert
        Assert.That(activationCode, Is.Not.Null.And.Not.Empty);
        Assert.That(activationCode.Length, Is.EqualTo(6));
        Assert.That(activationCode, Does.Match(@"^\d{6}$"));
    }

    [Test]
    public async Task Should_Verify_Correct_Code()
    {
        // Arrange
        const string storedCode = "123456";
        const string providedCode = "123456";

        // Act
        var isValid = await _smsHelper.VerifyCode(storedCode, providedCode);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public async Task Should_Reject_Incorrect_Code()
    {
        // Arrange
        const string storedCode = "123456";
        const string providedCode = "654321";

        // Act
        var isValid = await _smsHelper.VerifyCode(storedCode, providedCode);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public async Task Should_Detect_Expired_Code()
    {
        // Arrange
        var codeCreatedAt = DateTime.UtcNow.AddMinutes(-10); // 10 minutes ago
        const int expirationMinutes = 5;

        // Act
        var isExpired = await _smsHelper.IsCodeExpired(codeCreatedAt, expirationMinutes);

        // Assert
        Assert.That(isExpired, Is.True);
    }

    [Test]
    public async Task Should_Detect_Valid_Code_Not_Expired()
    {
        // Arrange
        var codeCreatedAt = DateTime.UtcNow.AddMinutes(-2); // 2 minutes ago
        const int expirationMinutes = 5;

        // Act
        var isExpired = await _smsHelper.IsCodeExpired(codeCreatedAt, expirationMinutes);

        // Assert
        Assert.That(isExpired, Is.False);
    }
}
```

#### 6. Integration Testing with In-Memory Database
```csharp
[TestFixture]
public class AuthServiceIntegrationTests
{
    private DbContext _context;
    private IRepository<User, Guid> _userRepository;
    private IRepository<EmailAuthenticator<Guid>, Guid> _emailAuthRepository;
    private AuthService _authService;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _userRepository = new Repository<User, Guid>(_context);
        _emailAuthRepository = new Repository<EmailAuthenticator<Guid>, Guid>(_context);

        var hashingService = new BCryptHashingService(4);
        var emailHelper = new EmailAuthenticatorHelper();
        var emailService = new Mock<IEmailService>();

        var emailAuthService = new EmailAuthService(emailHelper, _emailAuthRepository, emailService.Object);

        _authService = new AuthService(hashingService, emailAuthService);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task Should_Register_User_And_Send_Email_Verification()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "TestPassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var response = await _authService.RegisterAsync(request);
        var activationKey = await _authService.SendEmailVerificationAsync(response.UserId, request.Email);

        // Assert
        Assert.That(response.UserId, Is.Not.EqualTo(Guid.Empty));
        Assert.That(activationKey, Is.Not.Null.And.Not.Empty);

        var emailAuth = await _emailAuthRepository.GetAsync(e => e.UserId == response.UserId);
        Assert.That(emailAuth, Is.Not.Null);
        Assert.That(emailAuth.IsVerified, Is.False);
        Assert.That(emailAuth.ActivationKey, Is.EqualTo(activationKey));
    }
}
```

### Unit Testing with Test Builders

#### 1. Entity Builder Usage
```csharp
public class UserBuilder : EntityBuilder<User, Guid, UserBuilder>
{
    private Email _email = Email.Create("test@example.com");
    private string _firstName = "John";
    private string _lastName = "Doe";

    protected override User CreateEntity()
    {
        return new User(_email, _firstName, _lastName);
    }

    public UserBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }

    public UserBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }
}

// Usage in tests
[Test]
public void Should_Create_User_With_Valid_Email()
{
    // Arrange
    var user = new UserBuilder()
        .WithEmail("john.doe@example.com")
        .WithName("John", "Doe")
        .WithCreatedDate(DateTime.UtcNow)
        .Build();

    // Act & Assert
    Assert.That(user.Email.Value, Is.EqualTo("john.doe@example.com"));
    Assert.That(user.FirstName, Is.EqualTo("John"));
    Assert.That(user.LastName, Is.EqualTo("Doe"));
}
```

#### 2. In-Memory Database Testing
```csharp
[TestFixture]
public class UserRepositoryTests
{
    private ApplicationDbContext _context;
    private IAsyncRepository<User, Guid> _userRepository;

    [SetUp]
    public void Setup()
    {
        _context = InMemoryDbContextFactory.Create<ApplicationDbContext>();
        _userRepository = new EfRepositoryBase<User, Guid, ApplicationDbContext>(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task Should_Add_User_Successfully()
    {
        // Arrange
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .Build();

        // Act
        var result = await _userRepository.AddAsync(user);

        // Assert
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));

        var savedUser = await _userRepository.GetAsync(u => u.Id == result.Id);
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser.Email.Value, Is.EqualTo("test@example.com"));
    }

    [Test]
    public async Task Should_Get_Users_With_Specification()
    {
        // Arrange
        var activeUser = new UserBuilder().WithEmail("active@example.com").Build();
        var inactiveUser = new UserBuilder().WithEmail("inactive@example.com").Build();
        inactiveUser.Deactivate();

        await _userRepository.AddAsync(activeUser);
        await _userRepository.AddAsync(inactiveUser);

        // Act
        var activeUsers = await _userRepository.GetListBySpecificationAsync(
            new ActiveUserSpecification()
        );

        // Assert
        Assert.That(activeUsers.Items.Count, Is.EqualTo(1));
        Assert.That(activeUsers.Items.First().Email.Value, Is.EqualTo("active@example.com"));
    }
}
```

#### 3. Integration Testing with Seeded Data
```csharp
[TestFixture]
public class UserServiceIntegrationTests
{
    private ApplicationDbContext _context;
    private UserService _userService;

    [SetUp]
    public void Setup()
    {
        _context = InMemoryDbContextFactory.CreateWithData<ApplicationDbContext>(SeedTestData);
        var userRepository = new EfRepositoryBase<User, Guid, ApplicationDbContext>(_context);
        var hashingService = new BCryptHashingService();
        _userService = new UserService(userRepository, hashingService);
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        var users = new[]
        {
            new UserBuilder().WithEmail("user1@example.com").Build(),
            new UserBuilder().WithEmail("user2@example.com").Build(),
            new UserBuilder().WithEmail("user3@example.com").Build()
        };

        context.Users.AddRange(users);
    }

    [Test]
    public async Task Should_Get_All_Active_Users()
    {
        // Act
        var users = await _userService.GetActiveUsersAsync();

        // Assert
        Assert.That(users.Count, Is.EqualTo(3));
    }
}

## Logging Examples

### Structured Logging Setup

#### 1. Program.cs Configuration
```csharp
using ZCode.Core.Logging.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilogLogging(builder.Configuration);

var app = builder.Build();

// Log application startup
Log.Information("Application starting up");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

#### 2. appsettings.json Configuration
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithEnvironmentName", "WithProcessId", "WithThreadId"]
  }
}
```

#### 3. Service Logging
```csharp
public class UserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IAsyncRepository<User, Guid> _userRepository;

    public UserService(ILogger<UserService> logger, IAsyncRepository<User, Guid> userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        _logger.LogInformation("Creating user with email {Email}", request.Email);

        try
        {
            var user = new User(Email.Create(request.Email), request.FirstName, request.LastName);
            var result = await _userRepository.AddAsync(user);

            _logger.LogInformation("User created successfully with ID {UserId}", result.Id);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user with email {Email}", request.Email);
            throw;
        }
    }

    public async Task<IPaginate<User>> GetUsersAsync(int page, int size)
    {
        using var scope = _logger.BeginScope("Getting users page {Page} size {Size}", page, size);

        _logger.LogDebug("Fetching users from repository");

        var users = await _userRepository.GetListAsync(
            index: page,
            size: size,
            orderBy: q => q.OrderBy(u => u.CreatedDate)
        );

        _logger.LogInformation("Retrieved {Count} users out of {Total}",
            users.Items.Count, users.Count);

        return users;
    }
}

## Background Jobs Examples

### Hangfire Integration

#### 1. Service Registration
```csharp
// Program.cs
using ZCode.Core.BackgroundJobs.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add background jobs
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddBackgroundJobs(connectionString);

var app = builder.Build();

// Add Hangfire dashboard (optional)
app.UseHangfireDashboard("/hangfire");
```

#### 2. Background Job Service Usage
```csharp
public class EmailService
{
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IBackgroundJobService backgroundJobService, ILogger<EmailService> logger)
    {
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    // Fire and forget job
    public void SendWelcomeEmail(string email, string userName)
    {
        _backgroundJobService.Enqueue(() => ProcessWelcomeEmail(email, userName));
    }

    // Delayed job
    public void SendReminderEmail(string email, TimeSpan delay)
    {
        _backgroundJobService.Schedule(() => ProcessReminderEmail(email), delay);
    }

    // Recurring job
    public void SetupDailyReports()
    {
        _backgroundJobService.AddOrUpdateRecurringJob(
            "daily-reports",
            () => GenerateDailyReport(),
            "0 9 * * *" // Every day at 9 AM
        );
    }

    // Background job methods
    public async Task ProcessWelcomeEmail(string email, string userName)
    {
        _logger.LogInformation("Sending welcome email to {Email}", email);

        // Email sending logic
        await Task.Delay(1000); // Simulate email sending

        _logger.LogInformation("Welcome email sent successfully to {Email}", email);
    }

    public async Task ProcessReminderEmail(string email)
    {
        _logger.LogInformation("Sending reminder email to {Email}", email);

        // Email sending logic
        await Task.Delay(1000);

        _logger.LogInformation("Reminder email sent successfully to {Email}", email);
    }

    public async Task GenerateDailyReport()
    {
        _logger.LogInformation("Generating daily report");

        // Report generation logic
        await Task.Delay(5000);

        _logger.LogInformation("Daily report generated successfully");
    }
}
```

#### 3. Controller Usage
```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserService _userService;
    private readonly EmailService _emailService;

    public UsersController(UserService userService, EmailService emailService)
    {
        _userService = userService;
        _emailService = emailService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        var user = await _userService.CreateUserAsync(request);

        // Send welcome email in background
        _emailService.SendWelcomeEmail(user.Email.Value, user.FirstName);

        // Send reminder email after 24 hours
        _emailService.SendReminderEmail(user.Email.Value, TimeSpan.FromHours(24));

        return Ok(new { UserId = user.Id });
    }
}

## Exception Handling Examples

### Global Exception Handling

#### 1. Middleware Setup
```csharp
// Program.cs
using ZCode.Core.CrossCuttingConcerns.Exception.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Add global exception handling middleware
app.ConfigureCustomExceptionMiddleware();
```

#### 2. Custom Exceptions Usage
```csharp
public class UserService
{
    public async Task<User> GetUserByIdAsync(Guid id)
    {
        var user = await _userRepository.GetAsync(u => u.Id == id);

        if (user == null)
        {
            throw new NotFoundException($"User with ID {id} not found");
        }

        return user;
    }

    public async Task<User> CreateUserAsync(CreateUserRequest request)
    {
        // Business rule validation
        var existingUser = await _userRepository.GetAsync(u => u.Email.Value == request.Email);
        if (existingUser != null)
        {
            throw new BusinessException("User with this email already exists");
        }

        // Validation
        if (string.IsNullOrWhiteSpace(request.FirstName))
        {
            throw new ValidationException(new[]
            {
                new ValidationExceptionModel
                {
                    Property = nameof(request.FirstName),
                    Errors = new[] { "First name is required" }
                }
            });
        }

        var user = new User(Email.Create(request.Email), request.FirstName, request.LastName);
        return await _userRepository.AddAsync(user);
    }
}
```

#### 3. Authorization Exceptions
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        if (!_currentUserService.IsInRole("Admin"))
        {
            throw new AuthorizationException("You don't have permission to access this resource");
        }

        // Admin logic here
        return Ok();
    }
}
```

## Complete Project Setup

### 1. Project Structure
```
YourProject/
├── src/
│   ├── YourProject.Domain/
│   ├── YourProject.Application/
│   ├── YourProject.Infrastructure/
│   ├── YourProject.WebApi/
│   └── YourProject.Tests/
├── docs/
└── README.md
```

### 2. Package Installation
```bash
# Domain project
dotnet add package ZCode.Core.Domain

# Application project
dotnet add package ZCode.Core.Application

# Infrastructure project
dotnet add package ZCode.Core.Persistence
dotnet add package ZCode.Core.Security
dotnet add package ZCode.Core.Logging
dotnet add package ZCode.Core.BackgroundJobs

# WebApi project
dotnet add package ZCode.Core.CrossCuttingConcerns.Exception.WebApi

# Test project
dotnet add package ZCode.Core.Testing
```

### 3. Complete Program.cs Setup
```csharp
using ZCode.Core.Application.Extensions;
using ZCode.Core.Security.Extensions;
using ZCode.Core.Logging.Extensions;
using ZCode.Core.BackgroundJobs.Extensions;
using ZCode.Core.CrossCuttingConcerns.Exception.WebApi.Extensions;
using YourProject.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilogLogging(builder.Configuration);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Core services
builder.Services.AddApplicationServices(typeof(Program).Assembly);

// Add Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)
           .AddInterceptors(
               serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptors<Guid>>(),
               serviceProvider.GetRequiredService<DomainEventsInterceptor>()
           ));

builder.Services.AddPersistenceServices<ApplicationDbContext>();

// Add Security
builder.Services.AddSecurityServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);

// Add Background Jobs
builder.Services.AddBackgroundJobs(connectionString);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

// Add global exception handling
app.ConfigureCustomExceptionMiddleware();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Log application startup
Log.Information("Application starting up");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
```

### 4. Complete appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourProjectDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning",
        "Hangfire": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithEnvironmentName", "WithProcessId", "WithThreadId"]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 5. DbContext Configuration
```csharp
using Microsoft.EntityFrameworkCore;
using ZCode.Core.Persistence.Extensions;
using ZCode.Core.Domain.Entities;
using YourProject.Domain.Entities;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Register all entities that implement IEntity<Guid>
        modelBuilder.RegisterAllEntities<IEntity<Guid>>(typeof(User).Assembly);

        // Apply soft delete query filter
        modelBuilder.ApplySoftDeleteQueryFilter();

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
```

## Advanced Examples

### 1. Complex Domain Entity with All Features
```csharp
public class Order : AuditableEntity<Guid>
{
    public string OrderNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();

    private Order() { } // EF Core

    public Order(string orderNumber, Guid customerId)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Status = OrderStatus.Pending;
        TotalAmount = 0;

        // Pre-save event for validation
        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId, OrderNumber));
    }

    public void AddItem(Guid productId, int quantity, decimal unitPrice)
    {
        var item = new OrderItem(productId, quantity, unitPrice);
        Items.Add(item);
        RecalculateTotal();

        // Queue event for inventory check
        AddDomainEvent(new OrderItemAddedEvent(Id, productId, quantity));
    }

    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new BusinessException("Only pending orders can be confirmed");

        Status = OrderStatus.Confirmed;

        // Post-save event for notifications
        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, TotalAmount));
    }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Quantity * i.UnitPrice);
    }
}

// Domain Events
public class OrderCreatedEvent : DomainEvent, IPreSaveDomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public string OrderNumber { get; }

    public OrderCreatedEvent(Guid orderId, Guid customerId, string orderNumber)
    {
        OrderId = orderId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
    }
}

public class OrderConfirmedEvent : DomainEvent, IPostSaveDomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }

    public OrderConfirmedEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
}
```

### 2. Advanced CQRS with Caching and Validation
```csharp
// Command with validation and transaction
public class CreateOrderCommand : IRequest<Result<OrderDto>>, ITransactionalRequest, ICachableRequest
{
    public Guid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();

    // Caching configuration
    public bool BypassCache { get; set; }
    public string CacheKey => $"Order-Customer-{CustomerId}";
    public string? CacheGroupKey => "Orders";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);
}

// Command validator
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemDtoValidator());
    }
}

// Command handler with domain events
public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    private readonly IAsyncRepository<Order, Guid> _orderRepository;
    private readonly IAsyncRepository<Customer, Guid> _customerRepository;
    private readonly IMapperService _mapper;
    private readonly IDomainEventPublisher _eventPublisher;

    public CreateOrderCommandHandler(
        IAsyncRepository<Order, Guid> orderRepository,
        IAsyncRepository<Customer, Guid> customerRepository,
        IMapperService mapper,
        IDomainEventPublisher eventPublisher)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _mapper = mapper;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Validate customer exists
        var customer = await _customerRepository.GetAsync(c => c.Id == request.CustomerId);
        if (customer == null)
        {
            return Result.Failure<OrderDto>("Customer not found");
        }

        // Generate order number
        var orderNumber = await GenerateOrderNumberAsync();

        // Create order
        var order = new Order(orderNumber, request.CustomerId);

        // Add items
        foreach (var itemDto in request.Items)
        {
            order.AddItem(itemDto.ProductId, itemDto.Quantity, itemDto.UnitPrice);
        }

        // Save order
        var savedOrder = await _orderRepository.AddAsync(order, cancellationToken);

        // Queue additional events
        await _eventPublisher.QueueEventAsync(
            new OrderCreatedNotificationEvent(savedOrder.Id, customer.Email.Value),
            cancellationToken);

        var orderDto = _mapper.Map<OrderDto>(savedOrder);
        return Result.Success(orderDto);
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var count = await _orderRepository.CountAsync();
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{count + 1:D6}";
    }
}
```

### 3. Advanced Repository with Specifications
```csharp
// Complex specification
public class OrdersByCustomerAndStatusSpecification : Specification<Order>
{
    private readonly Guid _customerId;
    private readonly OrderStatus _status;
    private readonly DateTime? _fromDate;
    private readonly DateTime? _toDate;

    public OrdersByCustomerAndStatusSpecification(
        Guid customerId,
        OrderStatus status,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        _customerId = customerId;
        _status = status;
        _fromDate = fromDate;
        _toDate = toDate;
    }

    public override Expression<Func<Order, bool>> ToExpression()
    {
        var expression = PredicateBuilder.New<Order>(true);

        expression = expression.And(o => o.CustomerId == _customerId);
        expression = expression.And(o => o.Status == _status);

        if (_fromDate.HasValue)
            expression = expression.And(o => o.CreatedDate >= _fromDate.Value);

        if (_toDate.HasValue)
            expression = expression.And(o => o.CreatedDate <= _toDate.Value);

        return expression;
    }
}

// Repository usage with complex queries
public class OrderService
{
    private readonly IAsyncRepository<Order, Guid> _orderRepository;

    public async Task<IPaginate<Order>> GetCustomerOrdersAsync(
        Guid customerId,
        OrderStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 0,
        int size = 10)
    {
        if (status.HasValue)
        {
            var specification = new OrdersByCustomerAndStatusSpecification(
                customerId, status.Value, fromDate, toDate);

            return await _orderRepository.GetListBySpecificationAsync(
                specification,
                orderBy: q => q.OrderByDescending(o => o.CreatedDate),
                include: q => q.Include(o => o.Items),
                index: page,
                size: size
            );
        }

        // Use dynamic query for flexible filtering
        var dynamicQuery = new DynamicQuery
        {
            Filter = new Filter
            {
                Logic = "and",
                Filters = new List<Filter>
                {
                    new() { Field = "CustomerId", Operator = "eq", Value = customerId.ToString() }
                }
            },
            Sort = new[]
            {
                new Sort { Field = "CreatedDate", Dir = "desc" }
            }
        };

        if (fromDate.HasValue)
        {
            dynamicQuery.Filter.Filters.Add(new Filter
            {
                Field = "CreatedDate",
                Operator = "gte",
                Value = fromDate.Value.ToString("yyyy-MM-dd")
            });
        }

        return await _orderRepository.GetListByDynamicAsync(
            dynamicQuery,
            include: q => q.Include(o => o.Items),
            index: page,
            size: size
        );
    }
}
```

### 4. Advanced Event Handling with Nested Events
```csharp
// Event handler that triggers multiple nested events
public class OrderConfirmedEventHandler : INotificationHandler<OrderConfirmedEvent>
{
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IAsyncRepository<Customer, Guid> _customerRepository;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<OrderConfirmedEventHandler> _logger;

    public OrderConfirmedEventHandler(
        IDomainEventPublisher eventPublisher,
        IAsyncRepository<Customer, Guid> customerRepository,
        IBackgroundJobService backgroundJobService,
        ILogger<OrderConfirmedEventHandler> logger)
    {
        _eventPublisher = eventPublisher;
        _customerRepository = customerRepository;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    public async Task Handle(OrderConfirmedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing order confirmed event for order {OrderId}", notification.OrderId);

        try
        {
            // Get customer details
            var customer = await _customerRepository.GetAsync(c => c.Id == notification.CustomerId);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found for order {OrderId}",
                    notification.CustomerId, notification.OrderId);
                return;
            }

            // Queue immediate events
            await _eventPublisher.QueueEventAsync(
                new UpdateCustomerStatisticsEvent(notification.CustomerId, notification.TotalAmount),
                cancellationToken);

            await _eventPublisher.QueueEventAsync(
                new UpdateInventoryEvent(notification.OrderId),
                cancellationToken);

            // Schedule background jobs
            _backgroundJobService.Enqueue(() =>
                SendOrderConfirmationEmail(customer.Email.Value, notification.OrderId));

            _backgroundJobService.Schedule(() =>
                SendOrderReminderEmail(customer.Email.Value, notification.OrderId),
                TimeSpan.FromHours(24));

            // Check if customer qualifies for loyalty program
            if (await IsEligibleForLoyaltyProgram(customer.Id))
            {
                await _eventPublisher.QueueEventAsync(
                    new CustomerLoyaltyEligibleEvent(customer.Id),
                    cancellationToken);
            }

            _logger.LogInformation("Order confirmed event processed successfully for order {OrderId}",
                notification.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order confirmed event for order {OrderId}",
                notification.OrderId);
            throw;
        }
    }

    private async Task<bool> IsEligibleForLoyaltyProgram(Guid customerId)
    {
        // Complex business logic to check loyalty eligibility
        var customerOrders = await _customerRepository.Query()
            .Where(c => c.Id == customerId)
            .SelectMany(c => c.Orders)
            .Where(o => o.Status == OrderStatus.Confirmed)
            .ToListAsync();

        var totalSpent = customerOrders.Sum(o => o.TotalAmount);
        var orderCount = customerOrders.Count;

        return totalSpent >= 1000 || orderCount >= 10;
    }

    public async Task SendOrderConfirmationEmail(string email, Guid orderId)
    {
        _logger.LogInformation("Sending order confirmation email to {Email} for order {OrderId}",
            email, orderId);

        // Email sending logic
        await Task.Delay(1000);

        _logger.LogInformation("Order confirmation email sent successfully");
    }

    public async Task SendOrderReminderEmail(string email, Guid orderId)
    {
        _logger.LogInformation("Sending order reminder email to {Email} for order {OrderId}",
            email, orderId);

        // Email sending logic
        await Task.Delay(1000);

        _logger.LogInformation("Order reminder email sent successfully");
    }
}
```

### 5. Advanced Testing Scenarios
```csharp
[TestFixture]
public class OrderServiceIntegrationTests
{
    private ApplicationDbContext _context;
    private OrderService _orderService;
    private Mock<IDomainEventPublisher> _mockEventPublisher;
    private Mock<IBackgroundJobService> _mockBackgroundJobService;

    [SetUp]
    public void Setup()
    {
        _context = InMemoryDbContextFactory.CreateWithData<ApplicationDbContext>(SeedTestData);

        var orderRepository = new EfRepositoryBase<Order, Guid, ApplicationDbContext>(_context);
        var customerRepository = new EfRepositoryBase<Customer, Guid, ApplicationDbContext>(_context);

        _mockEventPublisher = new Mock<IDomainEventPublisher>();
        _mockBackgroundJobService = new Mock<IBackgroundJobService>();

        _orderService = new OrderService(
            orderRepository,
            customerRepository,
            _mockEventPublisher.Object,
            _mockBackgroundJobService.Object);
    }

    private void SeedTestData(ApplicationDbContext context)
    {
        var customer = new CustomerBuilder()
            .WithEmail("customer@example.com")
            .WithName("John", "Doe")
            .Build();

        var orders = new[]
        {
            new OrderBuilder()
                .WithCustomerId(customer.Id)
                .WithStatus(OrderStatus.Confirmed)
                .WithCreatedDate(DateTime.UtcNow.AddDays(-10))
                .Build(),
            new OrderBuilder()
                .WithCustomerId(customer.Id)
                .WithStatus(OrderStatus.Pending)
                .WithCreatedDate(DateTime.UtcNow.AddDays(-5))
                .Build()
        };

        context.Customers.Add(customer);
        context.Orders.AddRange(orders);
    }

    [Test]
    public async Task Should_Create_Order_And_Trigger_Events()
    {
        // Arrange
        var customer = await _context.Customers.FirstAsync();
        var command = new CreateOrderCommand
        {
            CustomerId = customer.Id,
            Items = new List<OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2, UnitPrice = 50.00m },
                new() { ProductId = Guid.NewGuid(), Quantity = 1, UnitPrice = 30.00m }
            }
        };

        // Act
        var result = await _orderService.CreateOrderAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.TotalAmount, Is.EqualTo(130.00m));

        // Verify events were published
        _mockEventPublisher.Verify(
            x => x.QueueEventAsync(It.IsAny<OrderCreatedNotificationEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify order was saved
        var savedOrder = await _context.Orders.FindAsync(result.Value.Id);
        Assert.That(savedOrder, Is.Not.Null);
        Assert.That(savedOrder.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Should_Get_Customer_Orders_With_Specification()
    {
        // Arrange
        var customer = await _context.Customers.FirstAsync();

        // Act
        var orders = await _orderService.GetCustomerOrdersAsync(
            customer.Id,
            OrderStatus.Confirmed,
            DateTime.UtcNow.AddDays(-15),
            DateTime.UtcNow);

        // Assert
        Assert.That(orders.Items.Count, Is.EqualTo(1));
        Assert.That(orders.Items.First().Status, Is.EqualTo(OrderStatus.Confirmed));
    }

    [Test]
    public async Task Should_Handle_Complex_Business_Rules()
    {
        // Arrange
        var customer = await _context.Customers.FirstAsync();
        var order = await _context.Orders.FirstAsync(o => o.CustomerId == customer.Id);

        // Act
        var result = await _orderService.ConfirmOrderAsync(order.Id);

        // Assert
        Assert.That(result.IsSuccess, Is.True);

        // Verify background jobs were scheduled
        _mockBackgroundJobService.Verify(
            x => x.Enqueue(It.IsAny<Expression<Func<Task>>>()),
            Times.AtLeastOnce);
    }
}
```

## Performance Optimization Examples

### 1. Efficient Querying with Projections
```csharp
// DTO for projections
public class OrderSummaryDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedDate { get; set; }
}

// Efficient query with projection
public class GetOrderSummariesQuery : IRequest<IPaginate<OrderSummaryDto>>, ICachableRequest
{
    public int Page { get; set; } = 0;
    public int Size { get; set; } = 10;
    public OrderStatus? Status { get; set; }

    public bool BypassCache { get; set; }
    public string CacheKey => $"OrderSummaries-{Page}-{Size}-{Status}";
    public string? CacheGroupKey => "OrderSummaries";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(15);
}

public class GetOrderSummariesQueryHandler : IRequestHandler<GetOrderSummariesQuery, IPaginate<OrderSummaryDto>>
{
    private readonly ApplicationDbContext _context;

    public GetOrderSummariesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IPaginate<OrderSummaryDto>> Handle(GetOrderSummariesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Orders
            .AsNoTracking() // Important for read-only operations
            .Include(o => o.Customer)
            .Where(o => o.DeletedDate == null); // Explicit soft delete check

        if (request.Status.HasValue)
        {
            query = query.Where(o => o.Status == request.Status.Value);
        }

        // Project to DTO to reduce data transfer
        var projectedQuery = query.Select(o => new OrderSummaryDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = $"{o.Customer.FirstName} {o.Customer.LastName}",
            TotalAmount = o.TotalAmount,
            Status = o.Status,
            CreatedDate = o.CreatedDate
        });

        return await projectedQuery
            .OrderByDescending(o => o.CreatedDate)
            .ToPaginateAsync(request.Page, request.Size, 0, cancellationToken);
    }
}
```

### 2. Bulk Operations for Performance
```csharp
public class BulkOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<BulkOrderService> _logger;

    public BulkOrderService(ApplicationDbContext context, ILogger<BulkOrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> BulkUpdateOrderStatusAsync(List<Guid> orderIds, OrderStatus newStatus)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Bulk update using raw SQL for performance
            var affectedRows = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE Orders SET Status = {0}, UpdatedDate = {1} WHERE Id IN ({2}) AND DeletedDate IS NULL",
                (int)newStatus,
                DateTime.UtcNow,
                string.Join(",", orderIds.Select(id => $"'{id}'")));

            _logger.LogInformation("Bulk updated {Count} orders to status {Status}", affectedRows, newStatus);

            // Publish events for updated orders
            var updatedOrders = await _context.Orders
                .Where(o => orderIds.Contains(o.Id))
                .ToListAsync();

            foreach (var order in updatedOrders)
            {
                order.AddDomainEvent(new OrderStatusChangedEvent(order.Id, newStatus));
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to bulk update order status");
            return Result.Failure("Failed to update orders");
        }
    }

    public async Task<Result> BulkCreateOrdersAsync(List<CreateOrderRequest> requests)
    {
        const int batchSize = 100;
        var batches = requests.Chunk(batchSize);

        foreach (var batch in batches)
        {
            var orders = batch.Select(request => new Order(
                GenerateOrderNumber(),
                request.CustomerId
            )).ToList();

            _context.Orders.AddRange(orders);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }
}
```

### 3. Caching Strategies
```csharp
// Cache removal strategy
public class OrderCacheInvalidationService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<OrderCacheInvalidationService> _logger;

    public OrderCacheInvalidationService(IMemoryCache cache, ILogger<OrderCacheInvalidationService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public void InvalidateOrderCaches(Guid orderId, Guid customerId)
    {
        var cacheKeys = new[]
        {
            $"Order-{orderId}",
            $"Order-Customer-{customerId}",
            "OrderSummaries",
            $"CustomerOrders-{customerId}"
        };

        foreach (var key in cacheKeys)
        {
            _cache.Remove(key);
            _logger.LogDebug("Removed cache key: {CacheKey}", key);
        }
    }
}

// Cache warming service
public class CacheWarmupService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CacheWarmupService> _logger;

    public CacheWarmupService(IServiceProvider serviceProvider, ILogger<CacheWarmupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting cache warmup");

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        try
        {
            // Warm up frequently accessed data
            await mediator.Send(new GetOrderSummariesQuery { Page = 0, Size = 50 }, cancellationToken);
            await mediator.Send(new GetActiveCustomersQuery { Page = 0, Size = 100 }, cancellationToken);

            _logger.LogInformation("Cache warmup completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cache warmup failed");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

## Best Practices and Patterns

### 1. Repository Pattern with Unit of Work
```csharp
public class OrderManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAsyncRepository<Order, Guid> _orderRepository;
    private readonly IAsyncRepository<Customer, Guid> _customerRepository;
    private readonly IAsyncRepository<Product, Guid> _productRepository;
    private readonly IDomainEventPublisher _eventPublisher;

    public OrderManagementService(
        IUnitOfWork unitOfWork,
        IAsyncRepository<Order, Guid> orderRepository,
        IAsyncRepository<Customer, Guid> customerRepository,
        IAsyncRepository<Product, Guid> productRepository,
        IDomainEventPublisher eventPublisher)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<Result<OrderDto>> ProcessComplexOrderAsync(ComplexOrderRequest request)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 1. Validate customer
            var customer = await _customerRepository.GetAsync(c => c.Id == request.CustomerId);
            if (customer == null)
            {
                return Result.Failure<OrderDto>("Customer not found");
            }

            // 2. Validate products and check inventory
            var productIds = request.Items.Select(i => i.ProductId).ToList();
            var products = await _productRepository.GetListAsync(p => productIds.Contains(p.Id));

            if (products.Items.Count != productIds.Count)
            {
                return Result.Failure<OrderDto>("Some products not found");
            }

            // 3. Create order
            var order = new Order(GenerateOrderNumber(), request.CustomerId);

            // 4. Add items with inventory check
            foreach (var itemRequest in request.Items)
            {
                var product = products.Items.First(p => p.Id == itemRequest.ProductId);

                if (product.StockQuantity < itemRequest.Quantity)
                {
                    return Result.Failure<OrderDto>($"Insufficient stock for product {product.Name}");
                }

                order.AddItem(itemRequest.ProductId, itemRequest.Quantity, product.Price);

                // Update inventory
                product.ReduceStock(itemRequest.Quantity);
                await _productRepository.UpdateAsync(product);
            }

            // 5. Apply discounts if applicable
            if (await customer.IsEligibleForDiscountAsync())
            {
                order.ApplyDiscount(0.1m); // 10% discount
            }

            // 6. Save order
            var savedOrder = await _orderRepository.AddAsync(order);

            // 7. Update customer statistics
            customer.UpdateOrderStatistics(order.TotalAmount);
            await _customerRepository.UpdateAsync(customer);

            // 8. Commit transaction
            await _unitOfWork.CommitTransactionAsync();

            // 9. Publish events after successful transaction
            await _eventPublisher.PublishAsync(new ComplexOrderProcessedEvent(savedOrder.Id, customer.Id));

            var orderDto = MapToDto(savedOrder);
            return Result.Success(orderDto);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            return Result.Failure<OrderDto>($"Failed to process order: {ex.Message}");
        }
    }
}
```

### 2. Error Handling and Resilience
```csharp
public class ResilientOrderService
{
    private readonly IAsyncRepository<Order, Guid> _orderRepository;
    private readonly ILogger<ResilientOrderService> _logger;
    private readonly IBackgroundJobService _backgroundJobService;

    public async Task<Result<Order>> CreateOrderWithRetryAsync(CreateOrderRequest request)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                var order = new Order(GenerateOrderNumber(), request.CustomerId);

                foreach (var item in request.Items)
                {
                    order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
                }

                var savedOrder = await _orderRepository.AddAsync(order);

                _logger.LogInformation("Order {OrderId} created successfully on attempt {Attempt}",
                    savedOrder.Id, retryCount + 1);

                return Result.Success(savedOrder);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                retryCount++;
                _logger.LogWarning("Concurrency conflict on attempt {Attempt}: {Error}",
                    retryCount, ex.Message);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Failed to create order after {MaxRetries} attempts", maxRetries);
                    return Result.Failure<Order>("Failed to create order due to concurrency conflicts");
                }

                // Wait before retry with exponential backoff
                await Task.Delay(TimeSpan.FromMilliseconds(Math.Pow(2, retryCount) * 100));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating order");
                return Result.Failure<Order>($"Failed to create order: {ex.Message}");
            }
        }

        return Result.Failure<Order>("Maximum retry attempts exceeded");
    }

    public async Task<Result> ProcessOrderWithFallbackAsync(Guid orderId)
    {
        try
        {
            // Primary processing logic
            await ProcessOrderPrimaryAsync(orderId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Primary order processing failed for {OrderId}, using fallback", orderId);

            try
            {
                // Fallback to background processing
                _backgroundJobService.Enqueue(() => ProcessOrderInBackgroundAsync(orderId));
                return Result.Success();
            }
            catch (Exception fallbackEx)
            {
                _logger.LogError(fallbackEx, "Fallback processing also failed for {OrderId}", orderId);
                return Result.Failure("Both primary and fallback processing failed");
            }
        }
    }

    public async Task ProcessOrderInBackgroundAsync(Guid orderId)
    {
        _logger.LogInformation("Processing order {OrderId} in background", orderId);

        // Background processing logic with more lenient error handling
        try
        {
            var order = await _orderRepository.GetAsync(o => o.Id == orderId);
            if (order != null)
            {
                // Process order
                order.Confirm();
                await _orderRepository.UpdateAsync(order);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Background processing failed for order {OrderId}", orderId);

            // Schedule retry
            _backgroundJobService.Schedule(
                () => ProcessOrderInBackgroundAsync(orderId),
                TimeSpan.FromMinutes(30));
        }
    }
}
```

### 3. Monitoring and Health Checks
```csharp
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["database"] = _context.Database.GetConnectionString(),
                ["timestamp"] = DateTime.UtcNow
            };

            return HealthCheckResult.Healthy("Database is accessible", data);
        }
        catch (Exception ex)
        {
            var data = new Dictionary<string, object>
            {
                ["error"] = ex.Message,
                ["timestamp"] = DateTime.UtcNow
            };

            return HealthCheckResult.Unhealthy("Database is not accessible", data);
        }
    }
}

// Register health checks
public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("database")
            .AddCheck("memory", () =>
            {
                var allocated = GC.GetTotalMemory(false);
                var data = new Dictionary<string, object>
                {
                    ["allocated"] = allocated,
                    ["gen0"] = GC.CollectionCount(0),
                    ["gen1"] = GC.CollectionCount(1),
                    ["gen2"] = GC.CollectionCount(2)
                };

                return allocated < 1024 * 1024 * 1024 // 1GB
                    ? HealthCheckResult.Healthy("Memory usage is normal", data)
                    : HealthCheckResult.Unhealthy("Memory usage is high", data);
            });

        return services;
    }
}
```

## Quick Reference Guide

### 🚀 Essential Commands

#### Package Installation
```bash
# Core packages
dotnet add package ZCode.Core.Domain
dotnet add package ZCode.Core.Application
dotnet add package ZCode.Core.Persistence

# Additional packages
dotnet add package ZCode.Core.Security
dotnet add package ZCode.Core.Logging
dotnet add package ZCode.Core.BackgroundJobs
dotnet add package ZCode.Core.Testing
dotnet add package ZCode.Core.CrossCuttingConcerns.Exception.WebApi
```

#### Service Registration
```csharp
// Program.cs - Essential setup
builder.Services.AddApplicationServices(typeof(Program).Assembly);
builder.Services.AddPersistenceServices<ApplicationDbContext>();
builder.Services.AddSecurityServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddBackgroundJobs(connectionString);

// Middleware
app.ConfigureCustomExceptionMiddleware();
app.UseAuthentication();
app.UseAuthorization();
```

### 📋 Common Patterns Cheat Sheet

#### 1. Entity Creation
```csharp
public class User : AuditableEntity<Guid>
{
    public Email Email { get; private set; }

    public User(Email email, string firstName, string lastName)
    {
        Email = email;
        // Add domain events
        AddDomainEvent(new UserCreatedEvent(Id, email.Value));
    }
}
```

#### 2. CQRS Command/Query
```csharp
// Command
public class CreateUserCommand : IRequest<Result<UserDto>>, ITransactionalRequest
{
    public string Email { get; set; }
    public string FirstName { get; set; }
}

// Query with caching
public class GetUserQuery : IRequest<UserDto>, ICachableRequest
{
    public Guid Id { get; set; }
    public string CacheKey => $"User-{Id}";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(30);
}
```

#### 3. Repository Usage
```csharp
// Basic operations
var user = await _userRepository.GetAsync(u => u.Id == id);
var users = await _userRepository.GetListAsync(index: 0, size: 10);
await _userRepository.AddAsync(user);
await _userRepository.UpdateAsync(user);

// With specifications
var activeUsers = await _userRepository.GetListBySpecificationAsync(
    new ActiveUserSpecification());

// Dynamic queries
var dynamicQuery = new DynamicQuery
{
    Filter = new Filter { Field = "Name", Operator = "contains", Value = "John" }
};
var results = await _userRepository.GetListByDynamicAsync(dynamicQuery);
```

#### 4. Event Handling
```csharp
// Pre-save event
public class UserValidationEvent : DomainEvent, IPreSaveDomainEvent { }

// Post-save event
public class UserNotificationEvent : DomainEvent, IPostSaveDomainEvent { }

// Event handler with nested events
public class UserCreatedEventHandler : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Queue nested events
        await _eventPublisher.QueueEventAsync(
            new SendWelcomeEmailEvent(notification.UserId),
            cancellationToken);
    }
}
```

#### 5. Background Jobs
```csharp
// Fire and forget
_backgroundJobService.Enqueue(() => SendEmail(email));

// Delayed
_backgroundJobService.Schedule(() => SendReminder(email), TimeSpan.FromHours(24));

// Recurring
_backgroundJobService.AddOrUpdateRecurringJob(
    "daily-reports",
    () => GenerateReport(),
    "0 9 * * *");
```

#### 6. Security
```csharp
// JWT generation
var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
var token = _jwtService.GenerateToken(claims);

// Password hashing
var hashedPassword = _hashingService.HashPassword(password);
var isValid = _hashingService.VerifyPassword(password, hashedPassword);

// Current user
var userId = _currentUserService.UserId;
var isAdmin = _currentUserService.IsInRole("Admin");
```

#### 7. Testing
```csharp
// Entity builder
var user = new UserBuilder()
    .WithEmail("test@example.com")
    .WithName("John", "Doe")
    .Build();

// In-memory database
var context = InMemoryDbContextFactory.Create<ApplicationDbContext>();
var repository = new EfRepositoryBase<User, Guid, ApplicationDbContext>(context);
```

### 🔧 Configuration Templates

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=YourDb;Trusted_Connection=true"
  },
  "JwtSettings": {
    "SecretKey": "your-32-character-secret-key-here",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers",
    "ExpirationMinutes": 60
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

#### DbContext
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.RegisterAllEntities<IEntity<Guid>>(Assembly.GetExecutingAssembly());
    modelBuilder.ApplySoftDeleteQueryFilter();
    base.OnModelCreating(modelBuilder);
}
```

### 🎯 Best Practices Summary

#### ✅ Do's
- Use `AsNoTracking()` for read-only queries
- Implement specifications for complex queries
- Use Result pattern for error handling
- Cache frequently accessed data
- Use domain events for decoupling
- Implement proper logging with structured data
- Use background jobs for long-running tasks
- Write comprehensive tests with builders

#### ❌ Don'ts
- Don't use entities directly in API responses
- Don't ignore soft delete in custom queries
- Don't forget to handle domain events timing
- Don't cache sensitive data
- Don't expose internal domain logic in controllers
- Don't use synchronous methods in async contexts
- Don't forget to dispose resources properly

### 📊 Performance Tips

1. **Database Queries**
   - Use projections for large datasets
   - Implement proper indexing
   - Use bulk operations for large updates
   - Consider read replicas for reporting

2. **Caching**
   - Cache at multiple levels (memory, distributed)
   - Implement cache invalidation strategies
   - Use cache warming for critical data
   - Monitor cache hit ratios

3. **Background Processing**
   - Use queues for decoupling
   - Implement retry mechanisms
   - Monitor job failures
   - Use appropriate job scheduling

4. **Memory Management**
   - Dispose DbContext properly
   - Use streaming for large files
   - Implement pagination
   - Monitor memory usage

### 🔍 Troubleshooting Guide

#### Common Issues

1. **Domain Events Not Firing**
   - Check if interceptors are registered
   - Verify event inheritance (IPreSaveDomainEvent/IPostSaveDomainEvent)
   - Ensure SaveChanges is called

2. **Caching Not Working**
   - Verify cache key generation
   - Check cache expiration settings
   - Ensure ICachableRequest is implemented

3. **Background Jobs Not Running**
   - Check Hangfire dashboard
   - Verify connection string
   - Check job registration

4. **Authentication Issues**
   - Verify JWT settings
   - Check token expiration
   - Ensure middleware order

Bu ətraflı nümunələr və quick reference guide sizin ZCode.CorePackages library-lərinin bütün xüsusiyyətlərini real layihələrdə necə istifadə edəcəyinizi göstərir. Performance optimization, error handling, caching strategies, monitoring və troubleshooting kimi enterprise-level məsələlər də əhatə edilmişdir.

