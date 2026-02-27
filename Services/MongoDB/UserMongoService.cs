using EndguinField.Models;
using MongoDB.Driver;

namespace EndguinField.Services.MongoDB;

public interface IUserMongoService
{
	Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
	Task CreateAsync(User userObj, CancellationToken cancellationToken = default);
	Task UpdateAsync(string id, User userIn, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public class UserMongoService : IUserMongoService
{
	private readonly IMongoCollection<User> _users;

	public UserMongoService(MongoDbService mongoDbService)
	{
		_users = mongoDbService.GetCollection<User>("users");
	}

	public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken = default) =>
		await _users.Find(_ => true).ToListAsync(cancellationToken);

	public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
		await _users.Find(e => e.Id == id).FirstOrDefaultAsync(cancellationToken);

	public async Task CreateAsync(User userObj, CancellationToken cancellationToken = default) =>
		await _users.InsertOneAsync(userObj, cancellationToken: cancellationToken);

	public async Task UpdateAsync(string id, User userIn, CancellationToken cancellationToken = default) =>
		await _users.ReplaceOneAsync(e => e.Id == id, userIn, cancellationToken: cancellationToken);

	public async Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
		await _users.DeleteOneAsync(e => e.Id == id, cancellationToken);
}
