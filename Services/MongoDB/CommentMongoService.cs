using EndguinField.Models;
using MongoDB.Driver;

namespace EndguinField.Services.MongoDB;

public interface ICommentMongoService
{
	Task<List<Comment>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
	Task CreateAsync(Comment comment, CancellationToken cancellationToken = default);
	Task UpdateAsync(string id, Comment commentIn, CancellationToken cancellationToken = default);
	Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public class CommentMongoService : ICommentMongoService
{
	private readonly IMongoCollection<Comment> _comments;

	public CommentMongoService(MongoDbService mongoDbService)
	{
		_comments = mongoDbService.GetCollection<Comment>("Comments");
	}

	public async Task<List<Comment>> GetAllAsync(CancellationToken cancellationToken = default) =>
		await _comments.Find(_ => true).ToListAsync(cancellationToken);

	public async Task<Comment?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
		await _comments.Find(c => c.Id == id).FirstOrDefaultAsync(cancellationToken);

	public async Task CreateAsync(Comment comment, CancellationToken cancellationToken = default) =>
		await _comments.InsertOneAsync(comment, cancellationToken: cancellationToken);

	public async Task UpdateAsync(string id, Comment commentIn, CancellationToken cancellationToken = default) =>
		await _comments.ReplaceOneAsync(c => c.Id == id, commentIn, cancellationToken: cancellationToken);

	public async Task DeleteAsync(string id, CancellationToken cancellationToken = default) =>
		await _comments.DeleteOneAsync(c => c.Id == id, cancellationToken);
}
