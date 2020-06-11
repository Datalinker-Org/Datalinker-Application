using DataLinker.Models.Enums;

namespace DataLinker.Services.Emails.Models
{
    public class UserVerificationLicenseEmail : CommonEmailProperties
    {
        public string Name { get; set; }
        public bool IsProvider { get; set; }
        public string OrganizationName { get; set; }
        public string SchemaName { get; set; }
        public string DetailsLink { get; set; }
        public PublishStatus DecisionMade { get; set; }
    }
}