using Ayura.API.Features.Community.DTOs;
using Ayura.API.Models;


namespace Ayura.API.Services;

public interface ICommunityService
{
    // 1. Get all communities
    Task<List<Community>> GetCommunities();

    // 2. Get a community by Id
    Task<Community> GetCommunities(string id);

    // 3. Create a Community
    Task<Community> CreateCommunity(Community community);

    // 4. Update a community
    Task UpdateCommunity(Community community);

    // 5. Delete a community
    Task DeleteCommunity(string id);

    // 6. Add a member
    Task AddMember(string communityId, string userId);
}