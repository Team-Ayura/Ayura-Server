using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Models;

[BsonIgnoreExtraElements]
public class OTP
{
    [BsonElement("mobileNumber")] public string MobileNumber { get; set; }

    [BsonElement("otp")] public string Otp { get; set; }

    [BsonElement("expiryTime")] public DateTime ExpiryTime { get; set; }
}