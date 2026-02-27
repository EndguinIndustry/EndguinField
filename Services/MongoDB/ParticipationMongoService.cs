using EndguinField.Models;
using MongoDB.Driver;

namespace EndguinField.Services.MongoDB;

public interface IParticipationMongoService
{
	Task<List<Participation>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
	Task<Participation?> GetAsync(string eventId, string userId, CancellationToken cancellationToken = default);
	Task<Participation> CreateAsync(Participation participation, CancellationToken cancellationToken = default);
	Task<bool> UpdateStatusAsync(string eventId, string userId, string status, CancellationToken cancellationToken = default);
	Task<bool> DeleteAsync(string eventId, string userId, CancellationToken cancellationToken = default);
	Task<bool> DeleteByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
}

public class ParticipationMongoService : IParticipationMongoService
{
	private const string CollectionName = "Participations";
	private readonly IMongoCollection<Participation> _collection;

	public ParticipationMongoService(MongoDbService mongoDbService)
	{
		_collection = mongoDbService.GetCollection<Participation>(CollectionName);
	}

	public async Task<List<Participation>> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
	{
		return await _collection.Find(p => p.EventId == eventId).ToListAsync(cancellationToken);
	}

	public async Task<Participation?> GetAsync(string eventId, string userId, CancellationToken cancellationToken = default)
	{
		return await _collection.Find(p => p.EventId == eventId && p.UserId == userId).FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<Participation> CreateAsync(Participation participation, CancellationToken cancellationToken = default)
	{
		if (participation is null) throw new ArgumentNullException(nameof(participation));

		participation.Id = null;
		await _collection.InsertOneAsync(participation, cancellationToken: cancellationToken);
		return participation;
	}

	public async Task<bool> UpdateStatusAsync(string eventId, string userId, string status, CancellationToken cancellationToken = default)
	{
		var update = Builders<Participation>.Update.Set(p => p.Status, status);
		var result = await _collection.UpdateOneAsync(p => p.EventId == eventId && p.UserId == userId, update, cancellationToken: cancellationToken);
		return result.ModifiedCount > 0;
	}

	public async Task<bool> DeleteAsync(string eventId, string userId, CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteOneAsync(p => p.EventId == eventId && p.UserId == userId, cancellationToken);
		return result.DeletedCount > 0;
	}

	public async Task<bool> DeleteByEventIdAsync(string eventId, CancellationToken cancellationToken = default)
	{
		var result = await _collection.DeleteManyAsync(p => p.EventId == eventId, cancellationToken);
		return result.DeletedCount > 0;
	}
}
