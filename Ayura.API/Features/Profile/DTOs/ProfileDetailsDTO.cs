namespace Ayura.API.Features.Profile.DTOs;

public class ProfileDetailsDTO
{
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;
    
    public DateTime BirthDay { get; set; } = DateTime.MinValue;
    
    public string Nationality { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
