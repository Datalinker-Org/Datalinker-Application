using DataLinker.Models;

namespace DataLinker.Services.Admin
{
    public interface IAdminService
    {
        AdminDashboardModel GetModelForAdmin(LoggedInUserDetails user);
    }
}
