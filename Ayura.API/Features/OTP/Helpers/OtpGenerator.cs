namespace Ayura.API.Features.OTP.Helpers;

public class OtpGenerator
{
    public static string GenerateOtp()
    {
        var random = new Random();
        var otp = random.Next(100000, 999999);
        return otp.ToString();
    }
}