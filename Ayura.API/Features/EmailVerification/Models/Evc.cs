using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

[BsonIgnoreExtraElements]
public class Evc
{
    [BsonElement("userId")] public string UserId { get; set; } = null!;

    [BsonElement("evc")] public string EvcReq { get; set; } = null!;

    [BsonElement("expiryTime")] public DateTime ExpiryTime { get; set; }

    [BsonElement("email")] public string Email { get; set; } = null!;
}