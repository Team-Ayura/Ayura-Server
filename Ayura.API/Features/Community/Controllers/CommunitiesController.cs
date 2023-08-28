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
    // 1. GET ALL Communities
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var allCommunities = await _communityService.GetCommunities();
        if (allCommunities.Any()) //Check whether there are any drivers in the collection
        {
            return Ok(allCommunities);
        }

        return NotFound(new { Message = "No Communities Found" });
    }

    // 2. GET a community by ID
    [HttpGet("{id:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> Get(string id)
    {
        var existingCommunity = await _communityService.GetCommunities(id);
        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        return Ok(existingCommunity);
    }

    // 3. Create a Community
    [HttpPost("create")]
    public async Task<IActionResult> CreateCommunity(CommunityModel community)
    {
        var createdCommunity = await _communityService.CreateCommunity(community);
        return CreatedAtAction("Get", new { id = createdCommunity.Id }, createdCommunity);
    }

    // 4. Update a Community
    [HttpPut("{id:length(24)}")]
    public async Task<IActionResult> UpdateCommunity(string id, CommunityModel updatedCommunity)
    {
        //Get the community from DB
        var existingCommunity = await _communityService.GetCommunities(id);

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

        return Ok(response);
    }

    // 5. Delete a community
    [HttpDelete("{id:length(24)}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existingCommunity = await _communityService.GetCommunities(id);

        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        await _communityService.DeleteCommunity(id);
        return NoContent();
    }

    //6. Adding a member to Community
    [HttpPut("addMember")]
    public async Task<IActionResult> AddMember([FromBody] MemberRequest memberRequest)
    {
        await _communityService.AddMember(memberRequest.CommunityId, memberRequest.UserId);
        
        return Ok(new { Message = "Member Added successfully." });
    }
}