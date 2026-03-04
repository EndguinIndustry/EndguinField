using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EndguinField.Models;

public class Tag
{
	[BsonId]
	[BsonRepresentation(BsonType.String)]
	public string Id { get; set; } = string.Empty;

	public string TagName { get; set; } = string.Empty;
}
