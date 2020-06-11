using System.Threading.Tasks;
using DataLinker.Models;

namespace DataLinker.Services.Identities
{
    public interface IIdsApiService
    {
        Task ChangeEmail(string subject, string emailAddress);

        Task<IdentityUser> CreateUser(string username, string plaintextPassword, string firstName, string phone);

        Task EditUser(string id, string firstName, string phone, bool active);
    }
}