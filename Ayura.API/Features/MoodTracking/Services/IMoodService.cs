namespace Ayura.API.Features.MoodTracking.Services;

public interface IMoodService
{
    Task<DailyMood> GetMoodsForDayAsync(string userId, DateTime date);
    Task<string> AddMoodsForDayAsync(string userId, MoodEntry moodEntry, DateTime date);
    Task<string> EditMoodsForDayAsync(string userId, DateTime date, List<MoodEntry> moodEntries);
}