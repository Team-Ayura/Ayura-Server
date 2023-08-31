namespace Ayura.API.Features.EmailVerification.Helpers;

// Generate a random string of length 8
public class EvcGenerator
{
    public static string GenerateEvc()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrst0123456789";
        var random = new Random();
        var evc = new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        return evc;
    }
}