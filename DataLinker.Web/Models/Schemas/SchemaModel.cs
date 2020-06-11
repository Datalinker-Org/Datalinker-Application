using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace DataLinker.Web.Models.Schemas
{
    public class SchemaModel: DataLinker.Models.SchemaModel
    {
        [Required]
        [Display(Name = "Schema name")]
        [Remote("IsSchemaNotExists", "Schemas", ErrorMessage = "Schema name already in use", AdditionalFields = "InitialName")]
        public new string Name { get; set; }

        [Required]
        [Display(Name = "Schema ID")]
        [Remote("IsSchemaIdNotExists", "Schemas", ErrorMessage = "Schema ID already in use", AdditionalFields = "initialId")]
        public new string PublicId { get; set; }

        [Required]
        public HttpPostedFileBase UploadFile { get; set; }
    }
}