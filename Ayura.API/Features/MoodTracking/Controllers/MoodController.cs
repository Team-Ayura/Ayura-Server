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

    [HttpGet("getmood/{date}")]
    public async Task<IActionResult> GetMoodsForDay(DateTime date)
    {
        Console.WriteLine("Controller called");
        var userId = HttpContext.Items["UserId"].ToString();
        Console.WriteLine(userId);

        var moodData = await _moodService.GetMoodsForDayAsync(userId, date);

        if (moodData == null)
        {
            Console.WriteLine("No mood data for the specified date");
            return NotFound("No mood data for the specified date");
        }

        Console.WriteLine("Mood data found");
        // response 200
        return Ok(moodData);
    }

    // addmood using AddMooodRequestDTO
    [HttpPost("addmood/{date}")]
    public async Task<IActionResult> AddMoodsForDay([FromBody] AddMoodRequestDTO addMoodRequest, DateTime date)
    {
        // get userid from httpcontext middleware
        var userId = HttpContext.Items["UserId"].ToString();
        Console.WriteLine(date);


        // map variables except date to moodentry object
        var moodEntry = new MoodEntry
        {
            Time = addMoodRequest.Time,
            MoodName = addMoodRequest.MoodName,
            MoodWeight = addMoodRequest.MoodWeight
        };

        // print moodentry object items
        Console.WriteLine(moodEntry.Time);
        Console.WriteLine(moodEntry.MoodName);
        Console.WriteLine(moodEntry.MoodWeight);

        // pass userid, moodentry object and date to service
        var moodAddStatus = await _moodService.AddMoodsForDayAsync(userId, moodEntry, date);

        if (moodAddStatus == null)
        {
            Console.WriteLine("Mood Adding Failed");
            return BadRequest("Mood Adding Failed");
        }

        // response 200
        return Ok(moodAddStatus);
    }
}