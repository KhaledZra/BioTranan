using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;

namespace BioTransWebApi.Services;

public enum MongoFilters
{
    Default,
    Equal,
    GreaterThan,
    LessThan,
    GreaterthanOrEqual,
    LessThanOrEqual
}

// T is the document/model that the service will handle
public class MongoDBService<T>
{
    // Collection field
    private readonly IMongoCollection<T> _collection;

    // IOptions<MongoDBSettings> mongoDbSettings
    public MongoDBService(MongoClient mongoClient,
        [FromServices] string databaseName,
        [FromServices] string collectionName)
    {
        IMongoDatabase database = mongoClient.GetDatabase(databaseName);
        _collection = database.GetCollection<T>(collectionName);
    }

    // Read
    public async Task<T> GetItemAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);

        return await _collection.Find(filter).Limit(1).FirstAsync();
    }

    public async Task<List<T>> GetItemsAsync()
    {
        return await _collection.Find(new BsonDocument()).ToListAsync();
    }

    // Let's MongoDb return exactly what we need.
    public async Task<List<T>> GetItemsWithCustomFilterAsync<TVal>(string field, TVal value,
        MongoFilters filters = MongoFilters.Default)
    {
        FilterDefinition<T> filter = Builders<T>.Filter.Empty;

        if (filters is MongoFilters.Default or MongoFilters.Equal) filter = Builders<T>.Filter.Eq(field, value);
        else if (filters is MongoFilters.GreaterThan) filter = Builders<T>.Filter.Gt(field, value);
        else if (filters is MongoFilters.LessThan) filter = Builders<T>.Filter.Lt(field, value);
        else if (filters is MongoFilters.GreaterthanOrEqual) filter = Builders<T>.Filter.Gte(field, value);
        else if (filters is MongoFilters.LessThanOrEqual) filter = Builders<T>.Filter.Lte(field, value);

        return await _collection.Find(filter).ToListAsync();
    }

    // Create
    public async Task CreateItemAsync(T item)
    {
        await _collection.InsertOneAsync(item);
    }

    public async Task CreateItemWithExpireDateAsync(T item)
    {
        await _collection.InsertOneAsync(item);

        // Create an index on the "expireAt" field with the "expireAfterSeconds" option
        var indexKeysDefinition = Builders<T>.IndexKeys.Ascending("ReservationShowing.ShowingDateAndTime");
        var indexOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.Zero }; // Will expire after two hours
        var indexModel = new CreateIndexModel<T>(indexKeysDefinition, indexOptions);
        await _collection.Indexes.CreateOneAsync(indexModel);
    }

    // Update
    public async Task ReplaceItemAsync(T item, string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        // alternative is UpdateOneAsync
        await _collection.ReplaceOneAsync(filter, item);
    }

    // Delete
    public async Task DeleteItemAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq("Id", id);
        await _collection.DeleteOneAsync(filter);
    }

    // Helper methods
    public async Task<bool> ConfirmItemIdAsync(string? id)
    {
        // Can be null if user enters whitespace
        if (string.IsNullOrWhiteSpace(id)) return false;

        // checks needs to match bson ObjectId
        if (!ObjectId.TryParse(id, out ObjectId _)) return false;

        var filter = Builders<T>.Filter.Eq("Id", id);
        bool exists = await _collection
            .Find(filter)
            .Limit(1)
            .AnyAsync();

        return exists;
    }
}