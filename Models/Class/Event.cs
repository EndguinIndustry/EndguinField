using MongoDB.Bson.Serialization.Attributes;

namespace EndguinField.Models;

[BsonIgnoreExtraElements]
public class Event
{
	public string Id { get; set; } = string.Empty;
	public string OwnerId { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public List<string> Images { get; set; } = new List<string>();
	public string BackgroundImage { get; set; } = string.Empty;
	public List<string> Participants { get; set; } = new List<string>();
	public string Location { get; set; } = string.Empty;

	[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
	public DateTime StartTime { get; set; }

	[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
	public DateTime EndTime { get; set; }

	public int MaxParticipants { get; set; }
	public List<string> Tags { get; set; } = new List<string>();
	public string Status { get; set; } = "Open"; // Open / Cancelled
	public List<string> BlackLists { get; set; } = new List<string>();
	public List<string> Comments { get; set; } = new List<string>();
}
