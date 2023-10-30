namespace Ayura.API.Features.MoodTracking.DTOs;

// DTO to get data from the request body and 
public class AddMoodRequestDTO
{
    public DateTime Date { get; set; }
    public string Time { get; set; } = null!;
    public string MoodName { get; set; } = null!;
    public int MoodWeight { get; set; }
}
