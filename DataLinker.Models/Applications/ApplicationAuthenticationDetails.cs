using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class ApplicationAuthenticationDetails
    {
        private const string UrlErrorMsg = "Invalid format. Please, enter full URL address like https://hostname.co.nz";
        public ApplicationAuthenticationDetails()
        {
        }

        public int ID { get; set; }

        public int ApplicationID { get; set; }

        // Application Authentication
        [Display(Name = "Well-known URL")]
        [DataType(DataType.Url, ErrorMessage = UrlErrorMsg)]
        public string WellKnownUrl { get; set; }
        
        [Display(Name = "Issuer")]
        [DataType(DataType.Url, ErrorMessage = UrlErrorMsg)]
        public string Issuer { get; set; }
        
        [Display(Name = "JWKS URL")]
        [DataType(DataType.Url, ErrorMessage = UrlErrorMsg)]
        public string JwksUri { get; set; }
        
        [Required]
        [Display(Name = "Authorisation URL")]
        [DataType(DataType.Url, ErrorMessage = UrlErrorMsg)]
        public string AuthorizationEndpoint { get; set; }
        
        [Required]
        [Display(Name = "Token URL")]
        [DataType(DataType.Url, ErrorMessage = UrlErrorMsg)]
        public string TokenEndpoint { get; set; }

        [Required]
        [Display(Name = "Revocation Endpoint")]
        [DataType(DataType.Url, ErrorMessage = UrlErrorMsg)]
        public string RevocationEndpoint { get; set; }

        [Required]
        [Display(Name = "Registration URL")]
        [DataType(DataType.Url, ErrorMessage = UrlErrorMsg)]
        public string RegistrationEndpoint { get; set; }
    }
}