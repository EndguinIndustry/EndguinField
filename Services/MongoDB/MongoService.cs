using MongoDB.Driver;
using Microsoft.Extensions.Options;

public class MongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService(IOptions<MongoDbSettings> settings)
    {
        var connectionString = settings.Value.ConnectionString;
        var databaseName = settings.Value.DatabaseName;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("MongoDB connection string is not configured. Set MongoDbSettings:ConnectionString (appsettings.json) or MONGO_CONNECTION_STRING (.env/env var).");
        }

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new InvalidOperationException("MongoDB database name is not configured. Set MongoDbSettings:DatabaseName (appsettings.json) or MONGO_DATABASE_NAME (.env/env var).");
        }

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoDatabase Database => _database;

    public IMongoCollection<T> GetCollection<T>(string collectionName)
    {
        return _database.GetCollection<T>(collectionName);
    }
}