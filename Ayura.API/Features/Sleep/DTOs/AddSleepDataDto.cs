namespace Ayura.API.Features.Sleep.DTOs;

public class AddSleepDataDto
{
    public string UserId { get; set; } = string.Empty;
    
    public DateTime BedTime { get; set; }
    
    public DateTime WakeupTime { get; set; }
    
    public string? Quality { get; set; }
    
    public List<string>? BeforeSleepAffect { get; set; }

    public List<string>? AfterSleepAffect { get; set; } 
}