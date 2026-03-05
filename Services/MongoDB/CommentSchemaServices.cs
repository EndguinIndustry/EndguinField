using System;
using MongoDB.Driver;
using CommentModel = EndguinField.Models.Comment;

public interface ICommentSchemaServices
{
    Task<List<CommentModel>> GetAllAsync(CancellationToken ct = default);
    Task<CommentModel> GetByIdAsync(string id, CancellationToken ct = default);
    Task<CommentModel> CreateAsync(CommentModel data, CancellationToken ct = default);
    Task UpdateAsync(string id, CommentModel updatedData, CancellationToken ct = default);
    Task DeleteAsync(string id, CancellationToken ct = default);
}

public class CommentSchemaServices : ICommentSchemaServices
{
    private const string CollectionName = "Comments";
    private readonly IMongoCollection<CommentModel> _collection;

    public CommentSchemaServices(MongoDbService mongoDb)
    {
        _collection = mongoDb.GetCollection<CommentModel>(CollectionName);
    }

    public async Task<List<CommentModel>> GetAllAsync(CancellationToken ct = default)
    {
        return await _collection.Find(_ => true).ToListAsync(ct);
    }

    public async Task<CommentModel> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Id cannot be null or empty.");
        var item = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
        if (item == null) throw new KeyNotFoundException("Comment not found.");
        return item;
    }

    public async Task<CommentModel> CreateAsync(CommentModel data, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(data.Id)) data.Id = Guid.NewGuid().ToString();
        if (data.TimeStamp == default) data.TimeStamp = DateTime.UtcNow;
        await _collection.InsertOneAsync(data, cancellationToken: ct);
        return data;
    }

    public async Task UpdateAsync(string id, CommentModel updatedData, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("Id cannot be null or empty.");
        updatedData.Id = id;
        var result = await _collection.ReplaceOneAsync(x => x.Id == id, updatedData, cancellationToken: ct);
        if (result.MatchedCount == 0) throw new KeyNotFoundException("Comment not found.");
    }

    public async Task DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id)) throw new ArgumentException("Id cannot be null or empty.");
        var result = await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken: ct);
        if (result.DeletedCount == 0) throw new KeyNotFoundException("Comment not found.");
    }
}
