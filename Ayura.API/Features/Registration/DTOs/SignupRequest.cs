namespace Ayura.API.Models.DTOs;

public class SignupRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDay { get; set; }
    public char Gender { get; set; }
    public string Nationality { get; set; }
    public int Height { get; set; }
    public double Weight { get; set; }
    public string BloodGroup { get; set; }
    public string ActivityLevel { get; set; }
}