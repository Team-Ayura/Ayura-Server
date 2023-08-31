using Ayura.API.Features.Community.DTOs;
using Ayura.API.Models;


namespace Ayura.API.Services;

public interface ICommunityService
{
    // 1. Get all Public communities
    Task<List<Community>> GetPublicCommunities();

    // 2. Get a community by Id
    Task<Community> GetCommunityById(string id);

    // 3. Create a Community
    Task<Community> CreateCommunity(Community community);

    // 4. Update a community
    Task UpdateCommunity(Community community);

    // 5. Delete a community
    Task DeleteCommunity(Community community);

    // 6. Add a member
    Task<Community> AddMember(string communityId, string userId);
}