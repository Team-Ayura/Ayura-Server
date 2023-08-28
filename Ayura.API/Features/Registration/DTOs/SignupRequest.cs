namespace Ayura.API.Models.DTOs;

public class SignupRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public DateTime BirthDay { get; set; }
    public char Gender { get; set; }
    public string Nationality { get; set; } = null!;
    public int Height { get; set; }
    public double Weight { get; set; }
    public string BloodGroup { get; set; } = null!;
    public string ActivityLevel { get; set; } = null!;
}