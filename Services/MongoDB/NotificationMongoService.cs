using EndguinField.Models;
using MongoDB.Driver;

namespace EndguinField.Services.MongoDB;

public interface INotificationMongoService
{
    Task<List<Notification>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Notification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Notification?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task CreateAsync(Notification notification, CancellationToken cancellationToken = default);
    Task UpdateAsync(string id, Notification notification, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public class NotificationMongoService : INotificationMongoService
{
    private readonly IMongoCollection<Notification> _notifications;

    public NotificationMongoService(MongoDbService mongoDbService)
    {
        _notifications = mongoDbService.GetCollection<Notification>("Notifications");
    }

    public async Task<List<Notification>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _notifications.Find(_ => true).ToListAsync(cancellationToken);

    public async Task<List<Notification>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
        await _notifications.Find(n => n.To == userId).SortByDescending(n => n.CreatedAt).ToListAsync(cancellationToken);

    public async Task<Notification?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        await _notifications.Find(n => n.Id == id).FirstOrDefaultAsync(cancellationToken);

    public async Task CreateAsync(Notification notification, CancellationToken cancellationToken = default) =>
        await _notifications.InsertOneAsync(notification, cancellationToken: cancellationToken);

    public async Task UpdateAsync(string id, Notification notification, CancellationToken cancellationToken = default) =>
        await _notifications.ReplaceOneAsync(n => n.Id == id, notification, cancellationToken: cancellationToken);

    public async Task MarkAsReadAsync(string id, CancellationToken cancellationToken = default)
    {
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _notifications.UpdateOneAsync(n => n.Id == id, update, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        await _notifications.DeleteOneAsync(n => n.Id == id, cancellationToken);
}
