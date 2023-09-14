using Ayura.API.Features.Activity.Models;

namespace Ayura.API.Features.Activity.DTOs;

public class AddCyclingRequest
{
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Distance { get; set; }
    public int Duration { get; set; }
    public int CaloriesBurned { get; set; }
    public List<LocationPoint>? Path { get; set; }
}