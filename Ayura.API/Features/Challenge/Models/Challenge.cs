using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

//Challenge Model
public class Challenge
{
    [BsonId] // to map Id property to _id attribute in mongodb
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")] //Mapping to MongoDB attributes
    public string Name { get; set; } = null!;
    
    [BsonElement("description")] public string? Description { get; set; }

    [BsonElement("duration")] public string Duration { get; set; } = null!;
    
    [BsonElement("distance")] public string Distance { get; set; } = null!;
    
    [BsonElement("type")] public string Type { get; set; } = null!;
    
    [BsonElement("leaderBoard")] public List<LeaderboardEntry>? LeaderBoard { get; set; }
    
    [BsonElement("communityId")] 
    [BsonRepresentation(BsonType.ObjectId)]
    public string CommunityId { get; set; } = null!;
    
    [BsonElement("createdDate")] public DateTime? CreatedDate { get; set; } 
    [BsonElement("endDate")] public DateTime? EndDate { get; set; } 
    
}