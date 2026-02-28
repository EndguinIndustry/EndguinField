using MongoDB.Driver;
using TagModel = EndguinField.Models.Tag;
public interface ITagSchemaServices
{
    Task<List<TagModel>> GetAllAsync(CancellationToken ct = default);
    
    Task<TagModel?> GetByIdAsync(string id, CancellationToken ct = default);
    
    Task<TagModel> CreateAsync(TagModel data, CancellationToken ct = default);
    
    Task<bool> UpdateAsync(string id, TagModel updatedData, CancellationToken ct = default);
    
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}

public class TagSchemaServices : ITagSchemaServices
{
	private const string CollectionName = "Tags";
    private readonly IMongoCollection<TagModel> _collection;

    public TagSchemaServices(MongoDbService mongoDb)
    {
        _collection = mongoDb.GetCollection<TagModel>(CollectionName);
    }

    public async Task<List<TagModel>> GetAllAsync(CancellationToken ct = default)
    {
        return await _collection.Find(_ => true).ToListAsync(ct);
    }

    public async Task<TagModel?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(ct);
    }

    public async Task<TagModel> CreateAsync(TagModel data, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(data.Id)) data.Id = Guid.NewGuid().ToString();
        await _collection.InsertOneAsync(data, cancellationToken: ct);
        return data;
    }

    public async Task<bool> UpdateAsync(string id, TagModel updatedData, CancellationToken ct = default)
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