using Ayura.API.Features.Community.DTOs;
using Ayura.API.Services;
using Microsoft.AspNetCore.Mvc;
using CommunityModel = Ayura.API.Models.Community; // Create an alias for the type

namespace Ayura.API.Features.Community.Controllers;

[ApiController]
[Route("api/communities")]
public class CommunitiesController : Controller
{
    private readonly CommunityService _communityService; // Getting Service

    // Community Service is injected on to the controller
    // This is Constructor as an arrow function
    public CommunitiesController(CommunityService communityService) => _communityService = communityService;


    // From here onwards methods
    // 1. GET ALL PUBLIC Communities
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var allCommunities = await _communityService.GetPublicCommunities();
        if (allCommunities.Any()) //Check whether there are any drivers in the collection
        {
            return Ok(allCommunities);
        }

        return NotFound(new { Message = "No Communities Found" });
    }

    // 2. GET a community by ID 
    [HttpGet("{communityId:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> Get(string communityId)
    {
        var existingCommunity = await _communityService.GetCommunityById(communityId);
        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        return Ok(existingCommunity);
    }

    // 3.  Get user joined communities 
    [HttpGet("joined/{userId:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> GetJoinedCommunities(string userId)
    {
        var joinedCommunities = await _communityService.GetJoinedCommunities(userId);
        if (joinedCommunities.Any())
        {
            return Ok(new
            {
                Message = "Community user joined",
                communities = joinedCommunities
            });
        }

        // Handle the case when the user is not found or has no joined communities.
        return NotFound(new
        {
            Message = "there are no joined communities or the user is not found."
        });
    }

    // 3. Create a Community
    [HttpPost("create")]
    public async Task<IActionResult> CreateCommunity(CommunityModel community)
    {
        var createdCommunity = await _communityService.CreateCommunity(community);
        return CreatedAtAction("Get", new { id = createdCommunity.Id }, createdCommunity);
    }

    // 4. Update a Community
    [HttpPut("{communityId:length(24)}")]
    public async Task<IActionResult> UpdateCommunity(string communityId, CommunityModel updatedCommunity)
    {
        //Get the community from DB
        var existingCommunity = await _communityService.GetCommunityById(communityId);

        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        updatedCommunity.Id = existingCommunity.Id;
        // Since this call Company Model the MembersList will be an empty String

        // Preserve old MembersList
        updatedCommunity.Members = existingCommunity.Members;
        await _communityService.UpdateCommunity(updatedCommunity);

        // Create the response object
        var response = new
        {
            Message = "Community updated successfully.",
            Community = updatedCommunity // Include the updated community
        };

        return Ok(updatedCommunity);
    }

    // 5. Delete a community
    [HttpDelete("{communityId:length(24)}")]
    public async Task<IActionResult> Delete(string communityId)
    {
        var existingCommunity = await _communityService.GetCommunityById(communityId);

        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        await _communityService.DeleteCommunity(existingCommunity);
        return Ok(new
        {
            Message = "Community deleted successfully.",
        });
    }

    //6. Adding a member to Community
    [HttpPut("addMember")]
    public async Task<IActionResult> AddMember([FromBody] MemberRequest memberRequest)
    {
        var community = await _communityService.AddMember(memberRequest.CommunityId, memberRequest.UserId);

        return community.Id == null
            ? NotFound(new { Message = "Member is already added" })
            : Ok(new { Message = "Member Added successfully.", Community = community });
    }
}