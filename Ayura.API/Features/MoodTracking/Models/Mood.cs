public class Mood
{
    public string Id { get; set; } // Generate a unique ID for each mood entry
    public DateTime Date { get; set; }
    public string Time { get; set; }
    public string MoodName { get; set; }
    public int MoodWeight { get; set; }
}