using Rezare.CommandBuilder;
using Rezare.CommandBuilder.Services;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLinker.Database.Models
{
    [TableName("Organizations")]
    public class Organization : IObject<int>
    {
        [Ignore, NotMapped]
        public string KeyName => "ID";

        [Ignore(IgnoreType.Insert | IgnoreType.Update)]
        public int ID { get; set; }

        public string Name { get; set; }

        public bool IsActive { get; set; }
        
        public string Phone { get; set; }

        public string Address { get; set; }

        public string AdministrativeContact { get; set; }

        public string AdministrativePhone { get; set; }

        public string TermsOfService { get; set; }

        public bool IsAgreeWithTerms { get; set; }

        [Ignore(IgnoreType.Update)]
        public DateTime CreatedAt { get; set; }
    }
}
