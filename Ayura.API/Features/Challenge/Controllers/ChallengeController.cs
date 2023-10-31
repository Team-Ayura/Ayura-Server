using Ayura.API.Services;
using Microsoft.AspNetCore.Mvc;
using ChallengeModel = Ayura.API.Models.Challenge; // Create an alias for the type
namespace Ayura.API.Features.Challenge.Controllers;

[ApiController]
[Route("api/challenges")]
public class ChallengeController : Controller
{
    private readonly ChallengeService _challengeService;

    public ChallengeController(ChallengeService challengeService)
    {
        _challengeService = challengeService;
    }
    
    //1.Get all challenges of a community
    [HttpGet("community/{communityId:length(24)}")]
    public async Task<IActionResult> GetChallenges(string communityId)
    {
        var allChallenges = await _challengeService.GetChallenges(communityId);
        if (allChallenges.Any())
        {
            return Ok(allChallenges);
        }

        return NotFound(new { Message = "There are no Challenges created in this community" });
        
    }
    
    //2. Get a challenge by ID 
    [HttpGet("{challengeId:length(24)}")]
    public async Task<IActionResult> GetChallengeById(string challengeId)
    {
        var existingChallenge = await _challengeService.GetChallengeById(challengeId);
        if (existingChallenge is null)
        {
            return NotFound(new { Message = "Challenge not found." });
        }

        return Ok(existingChallenge);
    }
    
    // 3. Create a Challenge
    [HttpPost("create")]
    public async Task<IActionResult> CreateChallenge(ChallengeModel challenge)
    {
        try
        {
            var createdChallenge = await _challengeService.CreateChallenge(challenge);
           return CreatedAtAction("GetChallengeById", new { challengeId = createdChallenge.Id }, createdChallenge);

        }
        catch (Exception e)
        {
            // Log the exception for debugging
            Console.WriteLine(e);

            // Return a more informative error response
            return StatusCode(500, "An error occurred while creating the challenge: " + e.Message);
        }
    }
    
    //4. start challenge
    [HttpPut("start/{challengeId:length(24)}/{memberId:length(24)}")]
    public async Task<IActionResult> StartChallenge(string challengeId, string memberId )
    {
        try
        {
            await _challengeService.StartChallenge(challengeId, memberId);
            return Ok("Challenge started successfully.");
        }
        catch (Exception e)
        {
            // Log the exception for debugging
            Console.WriteLine(e);

            // Return a more informative error response
            return StatusCode(500, "An error occurred while starting the challenge: " + e.Message);
        }
    }


}