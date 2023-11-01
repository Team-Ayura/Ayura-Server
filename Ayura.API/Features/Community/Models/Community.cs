using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

//Community Model
public class Community
{
    [BsonId] // to map Id property to _id attribute in mongodb
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("communityName")] //Mapping to MongoDB attributes
    public string CommunityName { get; set; } = null!;

    [BsonElement("communityDescription")] public string? CommunityDescription { get; set; }

    [BsonElement("isPublic")] public bool IsPublic { get; set; }

    [BsonElement("categories")] public List<string> Categories { get; set; } = null!;

    // Members of a community
    [BsonElement("members")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> Members { get; set; } = new List<string>();

    // Challenges of a community
    [BsonElement("challenges")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> Challenges { get; set; } = new List<string>();

    // AdminId
    [BsonElement("adminId")] //Mapping to MongoDB attributes
    [BsonRepresentation(BsonType.ObjectId)]
    public string AdminId { get; set; } = null!;
}