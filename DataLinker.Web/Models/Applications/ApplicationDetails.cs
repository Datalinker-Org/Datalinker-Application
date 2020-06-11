using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DataLinker.Web.Models.Applications
{
    public class ApplicationDetails: DataLinker.Models.ApplicationDetails
    {
        [Required]
        [Remote(action: "IsApplicationNotExistsForThisOrganization", controller:"Applications", ErrorMessage = "Application name already used in your organization", AdditionalFields = "InitialName")]
        public new string Name { get; set; }
    }
}