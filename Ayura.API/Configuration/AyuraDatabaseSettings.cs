using Ayura.API.Models.Configuration;

namespace Ayura.API.Configuration;

public class AyuraDatabaseSettings : IAyuraDatabaseSettings
{
    public string UserCollection { get; set; } = string.Empty;
    public string OtpCollection { get; set; } = string.Empty;
    public string EvcCollection { get; set; } = string.Empty;

    //CommunityCollection
    public string CommunityCollection { get; set; } = null!;
    public string PostCollection { get; set; } = string.Empty;
    public string CommentCollection { get; set; } = null!;
    public string ChallengeCollection { get; set; } = string.Empty;

    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}