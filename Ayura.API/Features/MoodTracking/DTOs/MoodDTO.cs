namespace Ayura.API.Features.MoodTracking.DTOs;

// DTO to get data from the request body and 
public class AddMoodRequestDTO
{
    // get moodEntry object and date
    public string Time { get; set; }
    public string MoodName { get; set; }
    public int MoodWeight { get; set; }
}