namespace RmvApiBackend.Settings
{
    /// <summary>
    /// This class holds the settings from the appsettings.json file.
    /// The property name "ApiKey" must match the key in the JSON file.
    /// </summary>
    public class RmvApiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
    }
}
