using System.Collections.Generic;

namespace DataLinker.Models
{
    public class DashboardModel
    {
        public int OrganizationID { get; set; }

        public string OrganizationName { get; set; }

        public bool IsActive { get; set; }

        public LegalRegistrationStatus LegalRegistration { get; set; }

        public List<ApplicationModel> PendingLegalApproval { get; set; }

        public List<ApplicationModel> OtherApplications { get; set; }

        public List<UserDetailsModel> Members { get; set; }

        public List<RegistrationModel> ConsumerLegelPendingApproval { get; set; }

        public DashboardModel()
        {
            ConsumerLegelPendingApproval = new List<RegistrationModel>();
        }
    }

    public class ApplicationModel
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public bool IsProvider { get; set; }
    }

    public class RegistrationModel
    {
        public string ConsumerApplicationName { get; set; }
    }

    public enum LegalRegistrationStatus
    {
        NotFound,
        NotCompleted,
        NotVerified,
        Completed
    }
}