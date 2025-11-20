using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Security.Claims;
using ZCode.Core.Domain.Entities;
using ZCode.Core.Persistence.Services;

namespace ZCode.Core.Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptors<TEntityId> : SaveChangesInterceptor
{
    private readonly IDateTimeService _dateTimeService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditableEntitySaveChangesInterceptors(IDateTimeService dateTimeService, IHttpContextAccessor httpContextAccessor)
    {
        _dateTimeService = dateTimeService;
        _httpContextAccessor = httpContextAccessor;
    }
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if ( eventData.Context != null)
            UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context != null)
            UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
    private void UpdateEntities(DbContext context)
    {
        if (context == null) return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity<TEntityId>>())
        {
            string? userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = ConvertUserId<TEntityId>(userId);
                    break;
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = ConvertUserId<TEntityId>(userId);
                    break;
                case EntityState.Deleted:
                    entry.Entity.DeletedBy = ConvertUserId<TEntityId>(userId);
                    break;
            }
        }
    }

    private static TId? ConvertUserId<TId>(string? userId)
    {
        if (userId == null)
            return default;

        try
        {
            // Əgər hədəf tip stringdirsə, sadəcə cast et
            if (typeof(TId) == typeof(string))
                return (TId)(object)userId;

            // Əgər hədəf tip Guiddirsə
            if (typeof(TId) == typeof(Guid) && Guid.TryParse(userId, out var guidValue))
                return (TId)(object)guidValue;

            // Əgər hədəf tip intdirsə
            if (typeof(TId) == typeof(int) && int.TryParse(userId, out var intValue))
                return (TId)(object)intValue;

            // Əgər hədəf tip long-dursa
            if (typeof(TId) == typeof(long) && long.TryParse(userId, out var longValue))
                return (TId)(object)longValue;

            // Əlavə parse tipləri bura əlavə oluna bilər

            throw new InvalidCastException($"Cannot convert userId to type {typeof(TId)}");
        }
        catch
        {
            return default;
        }
    }
}