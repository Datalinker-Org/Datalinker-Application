using PagedList;

namespace DataLinker.Web.Models.Users
{
    public class OrganisationUsers
    {
        public bool IncludeInActive { get; set; }

        public bool IsForSysAdmin { get; set; }

        public IPagedList<UserModel> Users { get; set; }
    }
}