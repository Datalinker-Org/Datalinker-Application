using DataLinker.Database.Models;
using DataLinker.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataLinker.Services.Users
{
    public interface IUserService
    {
        Organization AddNewOrganisationUser(int organizationId, UserDetailsModel model, LoggedInUserDetails loggedInUser);

        void ApproveLegalOfficerRegistration(int id, LoggedInUserDetails LoggedInUser);

        Task<User> ChangeEmailAddress(string token);

        bool CheckWhetherEmailInUse(string email, string InitialEmail);

        void DeclineLegalOfficerRegistration(int id, LoggedInUserDetails LoggedInUser);

        Task<string> EditUserDetails(int userId, UserDetailsModel model, LoggedInUserDetails loggedInUser);

        List<UserDetailsModel> GetOrganisationUsers(int organizationId, bool includeInActive, LoggedInUserDetails user);

        UserDetailsModel GetUserModel(int userId, LoggedInUserDetails user);

        User ResendEmailConfirmation(int id, LoggedInUserDetails LoggedInUser);

        Task<User> SaveUserCredentials(string token, string password);

        User SetupNewOrganisation(UserOrganizationModel model);

        void UpdateStatus(int id, bool value, LoggedInUserDetails loggedInUser);

        User GetUserByEmailConfirmationToken(string token);
    }
}