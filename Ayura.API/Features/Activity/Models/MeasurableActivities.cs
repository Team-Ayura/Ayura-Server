using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Features.Activity.Models;

public class MeasurableActivities
{
    [BsonElement("walkAndRunning")] public List<WalkAndRunningHistory> WalkAndRunning { get; set; } = null!;

    [BsonElement("cycling")] public List<CyclingHistory> Cycling { get; set; } = null!;
}