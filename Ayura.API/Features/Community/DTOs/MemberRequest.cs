namespace Ayura.API.Features.Community.DTOs;

public class MemberRequest
{
    public string CommunityId { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
}