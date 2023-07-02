namespace Ayura.API.Models.Configuration;

public class AyuraDatabaseSettings : IAyuraDatabaseSettings
{
    public string UserCollection { get; set; } = String.Empty;
    public string DatabaseName { get; set; } = String.Empty;
    public string ConnectionString { get; set; } = String.Empty;
}