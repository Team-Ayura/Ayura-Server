namespace Ayura.API.Features.Community.DTOs;

public class PostRequest
{
    public string PostId { get; set; } = null!;
    public string CommunityId { get; set; } = null!;
    public string AuthorId { get; set; } = null!;
    public string AuthorName { get; set; } = null!;
    public string Caption { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> Comments { get; set; }
}