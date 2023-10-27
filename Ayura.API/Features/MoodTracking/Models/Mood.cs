using Ayura.API.Models.Constants;

namespace Ayura.API.Features.MoodTracking.Models;


public class Mood
{
    public DateTime Date { get; set; }
    public MoodTypes MoodType { get; set; } = MoodTypes.Neutral;
}