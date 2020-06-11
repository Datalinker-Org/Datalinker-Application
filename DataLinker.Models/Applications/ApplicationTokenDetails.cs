namespace DataLinker.Models
{
    public class ApplicationTokenDetails
    {
        public int ID { get; set; }

        public int ApplicationID { get; set; }

        public string OriginHost { get; set; }

        public string Token { get; set; }

        public bool IsExpired { get; set; }
    }
}