using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("caption")] public string? Caption { get; set; }

    [BsonElement("imageUrl")] public string? ImageUrl { get; set; }

    [BsonElement("authorId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? AuthorId { get; set; }

    [BsonElement("communityId")] public string CommunityId { get; set; } = null!;


    [BsonElement("createdAt")] public DateTime CreatedAt { get; set; } // Timestamp property

    // Comments of a community
    [BsonElement("comments")]
    [BsonRepresentation(BsonType.ObjectId)]
    public List<string> Comments { get; set; } = new List<string>();
}