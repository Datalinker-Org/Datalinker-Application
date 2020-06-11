using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("dbo.Users")]
    public class User : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore.InsertAndUpdate]
        public int ID { get; set; }

        public string UserID { get; set; }

        public string Email { get; set; }

        public bool IsIntroducedAsLegalOfficer { get; set; }

        public bool IsVerifiedAsLegalOfficer { get; set; }

        public int? OrganizationID { get; set; }

        public bool IsSysAdmin { get; set; }

        public bool IsActive { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public string NewEmail { get; set; }

        public string Token { get; set; }

        public DateTime? TokenExpire { get; set; }

        [Ignore]
        public bool IsLegalOfficer => IsIntroducedAsLegalOfficer && IsVerifiedAsLegalOfficer;

        /// <summary>
        /// Wirtes newEmail to Email field. Sets IsAcive flag with true
        /// </summary>
        /// <param name="newEmail">Replace Email value with this value</param>
        public void UpdateEmailAddress(string newEmail)
        {
            Email = newEmail;
            IsActive = true;
            TokenExpire = null;
            NewEmail = null;
            Token = null;
        }
    }
}
