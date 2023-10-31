using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Features.Activity.Models;

public class CyclingHistory
{
    [BsonId] // to map Id property to _id attribute in mongodb
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("startTime")] public DateTime StartTime { get; set; }

    [BsonElement("endTime")] public DateTime EndTime { get; set; }

    [BsonElement("duration")] public int Duration { get; set; }

    [BsonElement("distance")] public double Distance { get; set; }

    [BsonElement("caloriesBurned")] public int CaloriesBurned { get; set; }

    [BsonElement("path")] public List<LocationPoint>? Path { get; set; }

    [BsonElement("images")] public List<string>? Images { get; set; }
}