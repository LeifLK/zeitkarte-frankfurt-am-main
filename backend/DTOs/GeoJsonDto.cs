using System.Text.Json.Serialization;

namespace backend.DTOs
{
    public class GeoJsonFeatureCollection
    {
        [JsonPropertyName("type")]
        public string Type => "FeatureCollection";

        [JsonPropertyName("features")]
        public List<GeoJsonFeature> Features { get; set; } = new();
    }

    public class GeoJsonFeature
    {
        [JsonPropertyName("type")]
        public string Type => "Feature";

        [JsonPropertyName("geometry")]
        public required GeoJsonGeometry Geometry { get; set; }

        [JsonPropertyName("properties")]
        public required Dictionary<string, object> Properties { get; set; }
    }

    public class GeoJsonGeometry
    {
        [JsonPropertyName("type")]
        public string Type => "Point";

        [JsonPropertyName("coordinates")]
        public required double[] Coordinates { get; set; }
    }
}