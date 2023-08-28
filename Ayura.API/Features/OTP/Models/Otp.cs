using MongoDB.Bson.Serialization.Attributes;

namespace Ayura.API.Features.OTP.Models;

[BsonIgnoreExtraElements]
public class Otp
{ [BsonElement("mobileNumber")] public string MobileNumber { get; set; } = null!;

    [BsonElement("otp")] public string OtpNum { get; set; } = null!;

    [BsonElement("expiryTime")] public DateTime ExpiryTime { get; set; } 
}