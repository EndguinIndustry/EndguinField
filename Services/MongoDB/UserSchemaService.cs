using MongoDB.Driver;

public interface IUserSchemaService
{
	Task<List<UserSchema>> GetAllAsync(CancellationToken cancellationToken = default);
	Task<UserSchema?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
	Task<UserSchema> CreateAsync(UserSchema user, CancellationToken cancellationToken = default);
	Task<bool> UpdateAsync(string id, UserSchema updated, CancellationToken cancellationToken = default);
	Task<string?> UploadPictureAsync(string id, IFormFile pic, CancellationToken cancellationToken = default);
	Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}

public class UserSchemaService : IUserSchemaService
{
	private const string CollectionName = "users";
	private readonly IMongoCollection<UserSchema> _collection;
	private readonly S3Service _s3;

	public UserSchemaService(MongoDbService mongoDb, S3Service s3)
	{
		_collection = mongoDb.GetCollection<UserSchema>(CollectionName);
		_s3 = s3;
	}

	public async Task<List<UserSchema>> GetAllAsync(CancellationToken cancellationToken = default)
	{
		return await _collection.Find(_ => true).ToListAsync(cancellationToken);
	}

	public async Task<UserSchema?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(id)) return null;
		return await _collection.Find(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
	}

	public async Task<UserSchema> CreateAsync(UserSchema user, CancellationToken cancellationToken = default)
	{
		if (user is null) throw new ArgumentNullException(nameof(user));

		// Let MongoDB generate ObjectId
		user.Id = null;
		await _collection.InsertOneAsync(user, cancellationToken: cancellationToken);
		return user;
	}

	public async Task<bool> UpdateAsync(string id, UserSchema updated, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(id)) return false;
		if (updated is null) throw new ArgumentNullException(nameof(updated));

		updated.Id = id;
		var result = await _collection.ReplaceOneAsync(
			x => x.Id == id,
			updated,
			cancellationToken: cancellationToken);

		return result.ModifiedCount == 1;
	}

	public async Task<string?> UploadPictureAsync(string id, IFormFile pic, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(id)) return null;
		if (pic is null || pic.Length == 0) return null;

		var exists = await _collection.Find(u => u.Id == id).AnyAsync(cancellationToken);
		if (!exists) return null;

		var picUrl = await _s3.UploadFileAsync(pic);
		var update = Builders<UserSchema>.Update.Set(x => x.ProfilePicture, picUrl);
		await _collection.UpdateOneAsync(x => x.Id == id, update, cancellationToken: cancellationToken);
		return picUrl;
	}

	public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
	{
		if (string.IsNullOrWhiteSpace(id)) return false;
		var result = await _collection.DeleteOneAsync(x => x.Id == id, cancellationToken);
		return result.DeletedCount == 1;
	}
}
