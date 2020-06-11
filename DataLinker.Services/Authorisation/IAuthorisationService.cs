using DataLinker.Models;
using System.Collections.Generic;

namespace DataLinker.Services.Authorisation
{
    public interface IAuthorisationService
    {
        LoggedInUserDetails GetUserDetails(List<System.Security.Claims.Claim> claims);
    }
}