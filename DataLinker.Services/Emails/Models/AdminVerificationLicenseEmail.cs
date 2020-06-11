using DataLinker.Models.Enums;

namespace DataLinker.Services.Emails.Models
{
    public class AdminVerificationLicenseEmail : CommonEmailProperties
    {
        public string Name { get; set; }

        public string DetailsLink { get; set; }

        public PublishStatus DecisionMade { get; set; }
    }
}