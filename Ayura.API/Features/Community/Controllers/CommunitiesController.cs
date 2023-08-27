using Ayura.API.Services;
using Microsoft.AspNetCore.Mvc;
using CommunityModel = Ayura.API.Models.Community; // Create an alias for the type

namespace Ayura.API.Features.Communities.Controllers;

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

        return NotFound();
    }

    // 2. GET a community by ID
    [HttpGet("{id:length(24)}")] // Constraint to check whether it has 24 chars
    public async Task<IActionResult> Get(string id)
    {
        var existingCommunity = await _communityService.GetCommunities(id);
        if (existingCommunity is null)
        {
            return NotFound();
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
    public async Task<IActionResult> UpdateCommunity(string id, CommunityModel community)
    {
        //Get the community from DB
        var existingCommunity = await _communityService.GetCommunities(id);

        if (existingCommunity is null)
        {
            return NotFound(new { Message = "Community not found." });
        }

        community.Id = existingCommunity.Id;

        await _communityService.UpdateCommunity(community);

        return NoContent();
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
    
}