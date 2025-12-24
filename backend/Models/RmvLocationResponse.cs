using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace RmvApiBackend.Models
{
    /// <summary>
    /// The top-level object in the location.name response.
    /// [JsonPropertyName] maps the JSON key (lowercase) to our C# property (PascalCase).
    /// </summary>
    public class RmvLocationResponse
    {
        [JsonPropertyName("stopLocationOrCoordLocation")]
        public List<StopOrCoordLocation> Locations { get; set; } = new();
    }

    /// <summary>
    /// A wrapper object. The API can return either a StopLocation or a CoordLocation.
    /// </summary>
    public class StopOrCoordLocation
    {
        [JsonPropertyName("StopLocation")]
        public StopLocation? StopLocation { get; set; }
    }

    /// <summary>
    /// Represents a specific transit stop (station, bus stop, etc.).
    /// </summary>
    public class StopLocation
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("extId")]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("lon")]
        public double Longitude { get; set; }

        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("dist")]
        public int Distance { get; set; }
    }
}
