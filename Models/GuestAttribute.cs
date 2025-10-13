namespace LocalRAG.Models
{
    public class GuestAttribute
    {
        public int Id { get; set; }
        public int GuestId { get; set; }
        public string AttributeKey { get; set; } = string.Empty;
        public string AttributeValue { get; set; } = string.Empty;

        public Guest? Guest { get; set; }
    }
}