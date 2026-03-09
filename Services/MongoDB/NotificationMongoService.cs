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
    Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default);
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
        var filter = Builders<Notification>.Filter.Eq(n => n.Id, id);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _notifications.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken cancellationToken = default)
    {
        var filter = Builders<Notification>.Filter.Eq(n => n.To, userId) & Builders<Notification>.Filter.Eq(n => n.IsRead, false);
        var update = Builders<Notification>.Update.Set(n => n.IsRead, true);
        await _notifications.UpdateManyAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
        await _notifications.DeleteOneAsync(n => n.Id == id, cancellationToken);
}
