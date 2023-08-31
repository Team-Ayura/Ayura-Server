namespace Ayura.API.Features.Activity.DTOs;

public class ActivityFilterRequest
{
    public string UserId { get; set; } = string.Empty;
    public string FilterType { get; set; } = string.Empty;
}