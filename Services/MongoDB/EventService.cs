using EndguinField.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EndguinField.Services.MongoDB
{
    public class EventService
    {
        private readonly IMongoCollection<Event> _eventCollection;

        public EventService(MongoDbService mongoDbService)
        {
            _eventCollection = mongoDbService.GetCollection<Event>("Events");
        }

        public async Task<List<Event>> GetAllEventsAsync()
        {
            return await _eventCollection.Find(_ => true).ToListAsync();
        }

        public async Task<Event?> GetEventByIdAsync(string id)
        {
            return await _eventCollection.Find(e => e.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ActionResult> UploadEventImageAsync(string imageUrl)
        {
            var eventDocument = new Event { ImageUrl = imageUrl };
            await _eventCollection.InsertOneAsync(eventDocument);
            return new OkResult();
        }

        public async Task<ActionResult> CreateEventAsync(Event eventData)
        {
            await _eventCollection.InsertOneAsync(eventData);
            return new OkResult();
        }

        public async Task<ActionResult> UpdateEventAsync(string id, Event eventData)
        {
            var filter = Builders<Event>.Filter.Eq(e => e.Id, id);
            await _eventCollection.ReplaceOneAsync(filter, eventData);
            return new OkResult();
        }

        public async Task<ActionResult> DeleteEventAsync(string id)
        {
            var filter = Builders<Event>.Filter.Eq(e => e.Id, id);
            await _eventCollection.DeleteOneAsync(filter);
            return new OkResult();
        }
    }
}
