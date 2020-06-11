using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.Security
{
    public interface ISecurityService
    {
        void CheckBasicAccess(LoggedInUserDetails user);

        Application CheckAccessToApplication(LoggedInUserDetails user, int applicationId);
    }
}
