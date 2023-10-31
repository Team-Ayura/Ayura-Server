using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Features.Sleep.Models;

public class SleepHistory
{
    [BsonId] // to map Id property to _id attribute in mongodb
    [BsonRepresentation(BsonType.ObjectId)]
    
    public string? Id { get; set; }
    
    [BsonElement("bedTime")] public DateTime BedTime { get; set; }
    
    [BsonElement("wakeupTime")] public DateTime WakeupTime { get; set; }
    
    [BsonElement("duration")] public int Duration { get; set; }

    [BsonElement("quality")] public string? Quality { get; set; }
    
    [BsonElement("beforeSleepAffect")] public List<string>? BeforeSleepAffect { get; set; }
    
    [BsonElement("afterSleepAffect")] public List<string>? AfterSleepAffect { get; set; }
    
    // Duration of sleep in hours
    [BsonElement("duration")] public double Duration { get; set; }

}