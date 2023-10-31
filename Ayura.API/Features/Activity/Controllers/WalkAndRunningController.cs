using Ayura.API.Features.Activity.DTOs;
using Ayura.API.Features.Activity.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.Activity.Controllers;

[Route("api/activity")]
[ApiController]
public class WalkAndRunningController : Controller
{
    private readonly IWalkAndRunningService _walkAndRunningService;

    public WalkAndRunningController(IWalkAndRunningService walkAndRunningService)
    {
        _walkAndRunningService = walkAndRunningService;
    }

    // 1. Get walk and running data 
    [HttpGet("getwalkandrundatabyfilter")]
    public async Task<IActionResult> GetWalkAndRunningDataByFilter(string userId, string filterType)
    {
        try
        {
            var data = await _walkAndRunningService.GetWalkAndRunningData(userId, filterType);
            return Ok(new { data });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // 2. Add walk and running data to database
    [HttpPost("addwalkandrunningdata")]
    public async Task<IActionResult> AddWalkAndRunningData([FromBody] AddWalkAndRunnigRequest addWalkAndRunningRequest)
    {
        try
        {
            await _walkAndRunningService.AddWalkAndRunningData(addWalkAndRunningRequest);
            return Ok(new { Status = "success" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // 3. Get today's improvement considering past data
    [HttpGet("getwalkandrunningimprovement")]
    public async Task<IActionResult> GetWalkAndRunningImprovement(string userId, int todayStepCount)
    {
        try
        {
            var data = await _walkAndRunningService.GetTodayImprovement(userId, todayStepCount);
            return Ok(new { data });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}