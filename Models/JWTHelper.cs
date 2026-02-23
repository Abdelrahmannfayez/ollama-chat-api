namespace ollama_chat_api.Models
{
    public class JWTHelper
    {
        public string key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string DurationInDays { get; set; }
    }
}
