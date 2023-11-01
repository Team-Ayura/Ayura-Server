namespace Ayura.API.Models.Configuration;

public interface IAyuraDatabaseSettings
{
    public string UserCollection { get; set; }
    public string OtpCollection { get; set; }
    public string EvcCollection { get; set; }
    public string CommunityCollection { get; set; } // COmmunityCollection
    public string PostCollection { get; set; }
    public string CommentCollection { get; set; } //commentCollection
    public string ChallengeCollection { get; set; }
    public string DatabaseName { get; set; }
    public string ConnectionString { get; set; }
}