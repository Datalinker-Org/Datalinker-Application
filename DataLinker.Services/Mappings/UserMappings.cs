using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.Mappings
{
    public static class UserMappings
    {
        public static LoggedInUserDetails ToModel(this User user)
        {
            var result = new LoggedInUserDetails
            {
                ID = user.ID,
                Email = user.Email,
                IsSysAdmin = user.IsSysAdmin,
                IsLegalRep = user.IsLegalOfficer,
                IsActive = user.IsActive
            };

            return result;
        }

        public static UserDetailsModel ToUserModel(this User user)
        {
            var result = new UserDetailsModel();
            result.ID = user.ID;
            result.Name = user.Name;
            result.IsLegalOfficer = user.IsLegalOfficer;
            result.OrganizationId = user.OrganizationID.Value;
            result.Email = user.Email;
            result.IsIntroducedAsLegalOfficer = user.IsIntroducedAsLegalOfficer;
            result.Phone = user.Phone;
            result.IsActive = user.IsActive;
            return result;
        }
    }
}