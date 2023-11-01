namespace Ayura.API.Features.Activity.DTOs;

public class AddWalkAndRunnigRequest
{
    public string UserId { get; set; } = string.Empty;
    public int StepCount { get; set; }
    public double DistanceWalked { get; set; }
    public int MoveMinutes { get; set; }
    public int CaloriesBurned { get; set; }
}