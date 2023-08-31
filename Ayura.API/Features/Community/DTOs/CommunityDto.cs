using Ayura.API.Models.Constants;

namespace Ayura.API.Features.Community.DTOs;

public class CommunityDto
{
    public string CommunityName { get; set; } = string.Empty;
    public string CommunityDescription { get; set; } = string.Empty;
    public bool IsPublic { get; set; } //  false = private, true = public
    // List of ChallengeCategories
    
}