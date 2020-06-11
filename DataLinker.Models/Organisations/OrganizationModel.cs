using System.ComponentModel;

namespace DataLinker.Models
{
    public class OrganizationModel
    {
        public OrganizationModel()
        {
        }      

        public int ID { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string Address { get; set; }

        public string AdministrativeContact { get; set; }

        public string AdministrativePhone { get; set; }

        [DisplayName("Agree with DataLinker terms")]
        public string TermsOfService { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }
}