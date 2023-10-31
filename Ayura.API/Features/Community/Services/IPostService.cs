using Ayura.API.Models;

namespace Ayura.API.Services;

public interface IPostService
{
    Task<List<Post>> GetPosts(string communityId);

    Task<Post> GetPost(string id);

    Task<Post> CreatePost(Post post);

    Task UpdatePost(Post post);

    Task DeletePost(string id);
}