public class DailyMood
{
    public DateTime Date { get; set; }

    public List<MoodEntry> MoodEntryList { get; set; }
}

public class MoodEntry
{
    public string Time { get; set; }
    public string MoodName { get; set; }
    public int MoodWeight { get; set; }
}