namespace Ayura.API.Features.Profile.DTOs;

public class UpdateDetailsDTO
{
    /* Details that should be allowed to change */
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
}