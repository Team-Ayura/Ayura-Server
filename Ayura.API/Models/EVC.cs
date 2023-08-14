using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

[BsonIgnoreExtraElements]
public class EVC
{
    [BsonElement("userId")] public string UserId { get; set; }

    [BsonElement("evc")] public string Evc { get; set; }

    [BsonElement("expiryTime")] public DateTime ExpiryTime { get; set; }

    [BsonElement("email")] public string Email { get; set; }
}