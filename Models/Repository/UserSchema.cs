using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserSchema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get; set;}
    public string Name {get; set;} = string.Empty;
    public int Age {get; set;}
    public string Gender {get; set;} = string.Empty;
    public string Description {get; set;} = string.Empty;
    public string ProfilePicture {get; set;} = string.Empty;
}