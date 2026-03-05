using EndguinField.Models;
using EndguinField.Services.MongoDB;

namespace EndguinField.Services;

public interface INotificationService
{
    Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10);
    Task<Notification?> GetNotificationAsync(string id);
    Task CreateNotificationAsync(string to, string detail, string type, string? relatedId = null);
    Task MarkAsReadAsync(string id);
    Task<bool> HasUnreadNotifications(string userId);
}

public class NotificationService : INotificationService
{
    private readonly INotificationMongoService _notificationStore;
    
    public NotificationService(INotificationMongoService notificationStore)
    {
        _notificationStore = notificationStore;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 10)
    {
        return await _notificationStore.GetByUserIdAsync(userId);
    }

    public async Task<Notification?> GetNotificationAsync(string id)
    {
        return await _notificationStore.GetByIdAsync(id);
    }
    
    public async Task CreateNotificationAsync(string to, string detail, string type, string? relatedId = null)
    {
        var notification = new Notification
        {
            Id = global::MongoDB.Bson.ObjectId.GenerateNewId().ToString(),
            To = to,
            Detail = detail,
            Type = type,
            RelatedEntityId = relatedId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        
        await _notificationStore.CreateAsync(notification);
    }
    
    public async Task MarkAsReadAsync(string id)
    {
        await _notificationStore.MarkAsReadAsync(id);
    }

    public async Task<bool> HasUnreadNotifications(string userId)
    {
        var notifications = await _notificationStore.GetByUserIdAsync(userId);
        return notifications.Any(n => !n.IsRead);
    }
}
