using System;
using MongoDB.Driver;
using CommentModel = EndguinField.Models.Comment;

public interface ICommentSchemaServices
{
    Task<List<CommentModel>> GetAllAsync(CancellationToken ct = default);

    Task<CommentModel?> GetByIdAsync(string id, CancellationToken ct = default);

    Task<CommentModel> CreateAsync(CommentModel data, CancellationToken ct = default);

    Task<bool> UpdateAsync(string id, CommentModel updatedData, CancellationToken ct = default);

    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
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

    public async Task<CommentModel?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<CommentModel> CreateAsync(CommentModel data, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(data.Id)) data.Id = Guid.NewGuid().ToString();
        if (data.TimeStamp == default) data.TimeStamp = DateTime.UtcNow;
        await _collection.InsertOneAsync(data, cancellationToken: ct);
        return data;
    }

    public async Task<bool> UpdateAsync(string id, CommentModel updatedData, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id)) return false;
        updatedData.Id = id;
        var result = await _collection.ReplaceOneAsync(x => x.Id == id, updatedData, cancellationToken: ct);
        return result.ModifiedCount == 1;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id)) return false;
        var result = await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken: ct);
        return result.DeletedCount == 1;
    }
}
