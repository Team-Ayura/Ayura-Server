using Ayura.API.Features.Activity.DTOs;
using Ayura.API.Features.Activity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.Activity.Controllers;

[Route("api/activity")]
[ApiController]
public class CyclingController : Controller
{
    private readonly ICyclingService _cyclingService;

    public CyclingController(ICyclingService cyclingService)
    {
        _cyclingService = cyclingService;
    }

    // 1. Get cycling data 
    [HttpGet("getcyclingdatabyfilter")]
    public async Task<IActionResult> GetCyclingDataByFilter(string userId, string filterType)
    {
        try
        {
            var data = await _cyclingService.GetCyclingData(userId, filterType);
            return Ok(new { data });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // 2. Add cycling data to database
    [HttpPost("addcyclingdata")]
    public async Task<IActionResult> AddCyclingData([FromBody] AddCyclingRequest addCyclingRequest)
    {
        try
        {
            await _cyclingService.AddCyclingData(addCyclingRequest);
            return Ok(new { Status = "success" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // 3. Get today's improvement considering past data
    [HttpGet("getcyclingimprovement")]
    public async Task<IActionResult> GetCyclingImprovement(string userId, int todayStepCount)
    {
        try
        {
            var data = await _cyclingService.GetTodayImprovement(userId, todayStepCount);
            return Ok(new { data });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}