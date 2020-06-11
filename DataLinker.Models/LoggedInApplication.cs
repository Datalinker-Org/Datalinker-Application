using System;

namespace DataLinker.Models
{
    public class LoggedInApplication
    {
        public LoggedInApplication()
        {
        }

        //public LoggedInApplication(Application application)
        //{
        //    ID = application.ID;
        //    Name = application.Name;
        //    PublicID = application.PublicID;
        //    IsIndustryGood = application.IsIntroducedAsIndustryGood && application.IsVerifiedAsIndustryGood;
        //}

        public bool IsIndustryGood { get; set; }

        public Guid PublicID { get; set; }

        public string Name { get; set; }

        public string TokenUsedToAuthorize { get; set; }

        public int ID { get; set; }

        public int UserId { get; set; }

        public bool IsProvider { get; set; }

        public LoggedInOrganization Organization { get; set; }
    }
}