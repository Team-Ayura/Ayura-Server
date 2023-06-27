using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

[BsonIgnoreExtraElements] // to ignore extra elements that are in mongo but not in model
public class User
{
    [BsonId] // to map Id property to _id attribute in mongodb
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("password")]
    public string Password { get; set; }
}