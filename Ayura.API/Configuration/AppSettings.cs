using Ayura.API.Models.Configuration;

namespace Ayura.API.Configuration;

public class AppSettings : IAppSettings
{
    public string SecretKey { get; set; } = string.Empty;
}