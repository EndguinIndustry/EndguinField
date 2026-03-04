using EndguinField.Models;
using MongoDB.Driver;

namespace EndguinField.Services.MongoDB;

public interface IEventMongoService
{
	Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<Event?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
	Task CreateAsync(Event eventObj, CancellationToken cancellationToken = default);
	Task UpdateAsync(string id, Event eventIn, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public class EventMongoService : IEventMongoService
{
	private readonly IMongoCollection<Event> _events;

	public EventMongoService(MongoDbService mongoDbService)
	{
		_events = mongoDbService.GetCollection<Event>("Events");
	}

	public async Task<List<Event>> GetAllAsync(CancellationToken cancellationToken = default) =>
		await _events.Find(_ => true).ToListAsync(cancellationToken);

	public async Task<Event?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
		await _events.Find(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);

	public async Task CreateAsync(Event eventObj, CancellationToken cancellationToken = default) =>
		await _events.InsertOneAsync(eventObj, cancellationToken: cancellationToken);

	public async Task UpdateAsync(string id, Event eventIn, CancellationToken cancellationToken = default) =>
		await _events.ReplaceOneAsync(e => e.Id == id, eventIn, cancellationToken: cancellationToken);

	public async Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
		await _events.DeleteOneAsync(e => e.Id == id, cancellationToken);
}
