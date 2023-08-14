namespace Ayura.API.Models.Configuration;

public class AppSettings : IAppSettings
{
    public string SecretKey { get; set; } = string.Empty;
}