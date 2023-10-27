using Ayura.API.Features.MoodTracking.Models;
using Ayura.API.Global.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.MoodTracking.Controllers;

[ApiController]
[Route("api/mood")]
public class MoodController : Controller
{
    

    
    [HttpPost("add")]
    public IActionResult AddMood([FromBody] Mood mood)
    {
        // get the user id from the jwt
        var userId = ResolveJwt.ResolveIdFromJwt(Request);
        
        // load the user model

        
        
        // if user is not logged in, return 401
        if (userId == null) return Unauthorized();
        
        
        // add the mood to the list
        // return the mood
     
        return Ok(mood);
    }


    
}