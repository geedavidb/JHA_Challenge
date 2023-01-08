using System.Text.Json.Serialization;

namespace JHA_Challenge.Models.Entity
{
    public class EntityAnnotation : AbstractEntity
    {
        public double Probability { get; init; }
        public string Type { get; init; }
        [JsonPropertyName("normalized_text")]
        public string NormalizedText { get; init; }
    }
}
