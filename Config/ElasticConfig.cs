namespace ClerkBot.Config
{
    public class ElasticConfig
    {
        public string Host { get; set; }
        public IndexConfig Indexs { get; set; }
        public bool UseAuthentication { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class IndexConfig
    {
        public string Conversations { get; set; }
        public string Users { get; set; }
        public string Electronics { get; set; }
    }
}
