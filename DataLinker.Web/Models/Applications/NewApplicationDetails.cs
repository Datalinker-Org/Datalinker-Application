using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace DataLinker.Web.Models.Applications
{
    public class NewApplicationDetails: DataLinker.Models.NewApplicationDetails
    {
        public const string MultipleUrls = @"((https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/*)((, ((https?:\/\/)?([\da-z\.-]+)\.([a-z\.]{2,6})([\/\w \.-]*)*\/*))*)";

        // New Application
        [Required]
        [Display(Name = "Name")]
        [Remote(action:"IsApplicationNotExistsForThisOrganization",controller: "Applications", ErrorMessage = "Application name already used in your organization")]
        public new string Name { get; set; }
    }
}