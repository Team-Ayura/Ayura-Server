// Controller to Get user mood, return mood data


using Ayura.API.Features.MoodTracking.DTOs;
using Ayura.API.Features.MoodTracking.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.MoodTracking;

[Route("api/mood")]
[ApiController]
public class MoodController : ControllerBase
{
    private readonly IMoodService _moodService;

    public MoodController(IMoodService moodService)
    {
        _moodService = moodService;
    }
    
    // 1. Get mood data from DTO and add to database
    [HttpPost("addmooddata")]
    public async Task<IActionResult> AddMoodData([FromBody] AddMoodRequestDTO addMoodRequestDto)
    {
        // get user id from context.Items["UserId"] = userId;
        string userId = HttpContext.Items["UserId"] as string;
        
        try
        {
            await _moodService.AddMoodData(addMoodRequestDto, userId);
            return Ok(new { Status = "success" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}