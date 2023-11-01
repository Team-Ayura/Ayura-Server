using Ayura.API.Features.Sleep.Models;

namespace Ayura.API.Features.Sleep.DTOs;

public class SleepDataResponse
{
    public string timePeriod { get; set; } = string.Empty;
    public double avgSleepTime { get; set; } = 0.0;
    public List<int> sleepingHours { get; set; } = new();

    public List<string> sleepQualities { get; set; } = new();

    public List<SleepHistory>? sleepHistory { get; set; }
}