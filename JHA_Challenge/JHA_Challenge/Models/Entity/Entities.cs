namespace JHA_Challenge.Models.Entity
{
    public class Entities
    {
        public EntityAnnotation[] Annotations { get; set; }
        public EntityLink[] Urls { get; set; }
        public EntityText[] Hashtags { get; set; }
        public EntityText[] Cashtags { get; set; }
        public EntityText[] Mentions { get; set; }
    }
}
