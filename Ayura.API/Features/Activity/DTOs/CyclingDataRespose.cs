using Ayura.API.Features.Activity.Models;

namespace Ayura.API.Features.Activity.DTOs;

public class CyclingDataRespose
{
    public string timePeriod { get; set; } = string.Empty;
    public double avgDistanceCycled { get; set; } = 0.0;
    public int avgCaloriesBurned { get; set; } = 0;
    public int avgDuration { get; set; } = 0;
    public int improvement { get; set; } = 0;
    public List<int> distances { get; set; } = new List<int>();

    public List<CyclingHistory>? cyclingHistory { get; set; }
}