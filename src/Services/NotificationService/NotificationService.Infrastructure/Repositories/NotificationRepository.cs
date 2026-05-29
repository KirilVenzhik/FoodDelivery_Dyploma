using Microsoft.EntityFrameworkCore;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using NotificationService.Infrastructure.Persistence;

namespace NotificationService.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _dbContext;

    public NotificationRepository(NotificationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task AddAsync(Notification notification, CancellationToken cancellationToken = default)
        => await _dbContext.Notifications.AddAsync(notification, cancellationToken);

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbContext.Notifications.FindAsync([id], cancellationToken);

    public async Task<List<Notification>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbContext.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _dbContext.SaveChangesAsync(cancellationToken);
}
