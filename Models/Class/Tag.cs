using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace EndguinField.Models;

public class Tag
{
	[BsonId] // บอกว่าเป็น Primary Key
	[BsonRepresentation(BsonType.ObjectId)] // บอกให้มอง string เป็น ObjectId ใน Mongo
	public string? Id { get; set; }
	public string TagName { get; set; } = string.Empty;
}
