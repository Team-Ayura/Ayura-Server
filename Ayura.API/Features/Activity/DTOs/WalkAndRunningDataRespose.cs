namespace Ayura.API.Features.Activity.DTOs;

public class WalkAndRunningDataRespose
{
    public string timePeriod { get; set; } = string.Empty;
    public double avgDistanceWalked { get; set; } = 0.0;
    public int avgCaloriesBurned { get; set; } = 0;
    public int avgMoveMinutes { get; set; } = 0;
    public int avgStepCount { get; set; } = 0;
    public int improvement { get; set; } = 0;
    public List<int> steps { get; set; } = new List<int>();
}