namespace Ayura.API.Models.Configuration;

public class AyuraDatabaseSettings : IAyuraDatabaseSettings
{
    public string UserCollection { get; set; } = string.Empty;
    public string OtpCollection { get; set; } = string.Empty;

    public string EvcCollection { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
}