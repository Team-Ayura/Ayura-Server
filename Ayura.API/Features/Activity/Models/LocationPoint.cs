using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Features.Activity.Models;

public class LocationPoint
{
    [BsonElement("latitude")] public double Latitude { get; set; }

    [BsonElement("longitude")] public double Longitude { get; set; }
}