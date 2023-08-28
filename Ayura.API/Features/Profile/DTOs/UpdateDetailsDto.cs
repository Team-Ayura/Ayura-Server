namespace Ayura.API.Features.Profile.DTOs;

public class UpdateDetailsDto
{
    /* Details that should be allowed to change */
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}