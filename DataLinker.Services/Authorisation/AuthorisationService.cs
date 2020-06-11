using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Services.Exceptions;
using Rezare.CommandBuilder.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataLinker.Services.Authorisation
{
    public class AuthorisationService : IAuthorisationService
    {
        private IService<User, int> _users;
        private IService<Organization, int> _organisations;

        public AuthorisationService(IService<User, int> users, IService<Organization, int> orgs)
        {
            _organisations = orgs;
            _users = users;
        }

        public LoggedInUserDetails GetUserDetails(List<System.Security.Claims.Claim> claims)
        {
            var userId = GetClaim(claims, "sub");
            // Get user
            var user = _users.FirstOrDefault(u => u.UserID == userId);

            var providedEmail = GetClaim(claims, "email");

            if (user == null || !string.Equals(providedEmail, user.Email, StringComparison.CurrentCultureIgnoreCase))
            {
                return null;
            }

            if (!user.IsActive)
            {
                throw new BaseException("User is not found or not active");
            }

            // Setup logged in user details
            var result = new LoggedInUserDetails
            {
                ID = user.ID,
                Email = user.Email,
                IsSysAdmin = user.IsSysAdmin,
                IsLegalRep = user.IsLegalOfficer,
                IsActive = user.IsActive
            };

            // Get organisation
            var org = _organisations.FirstOrDefault(i => i.ID == user.OrganizationID.Value);
            if (org == null)
            {
                throw new BaseException("Organisation was not found");
            }

            // Setup organisation
            result.Organization = new LoggedInOrganization
            {
                ID = org.ID,
                IsActive = org.IsActive,
                Name = org.Name
            };

            // Return result
            return result;
        }
        
        private string GetClaim(IEnumerable<System.Security.Claims.Claim> claims, string key)
        {
            var claim = claims.FirstOrDefault(c => c.Type == key);
            return claim == null ? null : claim.Value;
        }
    }
}
