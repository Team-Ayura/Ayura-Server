namespace Ayura.API.Features.EmailVerification.Helpers;

// Generate a random string of length 8
public class EVCGenerator
{
    public static string GenerateEVC()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrst0123456789";
        var random = new Random();
        var evc = new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        Console.WriteLine(evc);
        return evc;
    }
}