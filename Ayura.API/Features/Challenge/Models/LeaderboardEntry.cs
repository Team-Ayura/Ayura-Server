using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

public class LeaderboardEntry
{
    [BsonElement("memberId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string MemberId { get; set; } = null!;

    [BsonElement("score")] public int Score { get; set; } = 0;
}