namespace DataLinker.Models
{
    public class LoggedInOrganization
    {
        public LoggedInOrganization() { }

        public int ID { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }
    }
}