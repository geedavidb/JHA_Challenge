using JHA_Challenge.Models.Entity;

namespace JHA_Challenge.Models
{
    public class Tweet
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public Entities Entities { get; set; } = new Entities();
    }
}
