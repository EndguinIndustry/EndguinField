using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EndguinField.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        public string To { get; set; } = string.Empty;

        public string Detail { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? RelatedEntityId { get; set; }
        public string Type { get; set; } = "General"; 
    }
}
