using System.Text.Json.Serialization;

namespace JHA_Challenge.Models.Entity
{
    public class EntityLink : AbstractEntity
    {
        public string Url { get; init; }
        [JsonPropertyName("expanded_url")]
        public string ExpandedUrl { get; init; }
        [JsonPropertyName("display_url")]
        public string DisplayUrl { get; init; }
    }
}
