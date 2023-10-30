using Ayura.API.Features.Sleep.DTOs;
using Ayura.API.Features.Sleep.Services;
using Microsoft.AspNetCore.Mvc;


namespace Ayura.API.Features.Sleep.Controllers;
[Route("api/sleep")]
[ApiController]

public class SleepController : Controller
{
    private readonly ISleepService _sleepService;
    
    public SleepController(ISleepService sleepService)
    {
        _sleepService = sleepService;
    }
    
    // . Add sleep data to database
    [HttpPost("addsleepdata")]
    public async Task<IActionResult> AddSleepData([FromBody] AddSleepDataDto addSleepDataDto)
    {
        try
        {
            await _sleepService.AddSleepData(addSleepDataDto);
            return Ok(new { Status = "success" });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    // 2. Get sleeping data 
        [HttpGet("getsleepingdatabyfilter")]
        public async Task<IActionResult> GetSleepingDataByFilter(string userId, string filterType)
        {
            try
            {
                var data = await _sleepService.GetSleepingData(userId, filterType);
                return Ok(new { data });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

}