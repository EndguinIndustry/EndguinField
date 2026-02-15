using EndguinField.Models;
using MongoDB.Driver;

public class MongoDbService
{
    private readonly IMongoDatabase _database;

    public MongoDbService()
    {
        var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
        var databaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME");

        var client = new MongoClient(connectionString );
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<User> GetUsersCollection()
    {
        return _database.GetCollection<User>("Users");
    }
}