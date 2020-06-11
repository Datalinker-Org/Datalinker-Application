using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("ApplicationAuthentication")]
    public class ApplicationAuthentication : IObject<int>
    {
        public int ApplicationID { get; set; }

        public string WellKnownUrl { get; set; }

        public string Issuer { get; set; }

        public string JwksUri { get; set; }

        public string AuthorizationEndpoint { get; set; }

        public string TokenEndpoint { get; set; }

        public string UserInfoEndpoint { get; set; }

        public string EndSessionEndpoint { get; set; }

        public string CheckSessionIFrame { get; set; }

        public string RevocationEndpoint { get; set; }

        public string RegistrationEndpoint { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }

        [Ignore(IgnoreType.Update)]
        public int CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int? UpdatedBy { get; set; }

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        [Ignore, NotMapped]
        public string KeyName => "ID";
    }
}