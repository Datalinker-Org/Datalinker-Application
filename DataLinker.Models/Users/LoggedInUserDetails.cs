namespace DataLinker.Models
{
    public class LoggedInUserDetails
    {
        public LoggedInUserDetails()
        {
            IsActive = false;
            IsLegalRep = false;
            IsSysAdmin = false;
        }

        public int? ID { get; set; }

        public string Email { get; set; }

        public bool IsSysAdmin { get; set; }

        public bool IsLegalRep { get; set; }

        public bool IsActive { get; set; }

        public LoggedInOrganization Organization { get; set; }

        public bool HasOrganization => Organization != null;
    }
}