using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Features.Activity.Models;

public class WalkAndRunningHistory
{
    [BsonElement("date")] public DateTime Date { get; set; }
    
    [BsonElement("movementMinutes")] public int MoveMinutes { get; set; }
    
    [BsonElement("distanceWalked")] public double DistanceWalked { get; set; }
    
    [BsonElement("stepCount")] public int StepCount { get; set; }
    
    [BsonElement("caloriesBurned")] public int CaloriesBurned { get; set; }
}