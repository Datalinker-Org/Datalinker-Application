using System.Collections.Generic;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;

namespace DataLinker.IdentityServer
{
    internal static class Scopes
    {
        public static Scope[] Get()
        {
            return new[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.OfflineAccess,
                new Scope
                {
                    Name = "read",
                    DisplayName = "Read data",
                    Type = ScopeType.Resource,
                    Emphasize = false
                },
                new Scope
                {
                    Name = "write",
                    DisplayName = "Write data",
                    Type = ScopeType.Resource,
                    Emphasize = true
                },
                new Scope
                {
                    Name = "forbidden",
                    DisplayName = "Forbidden scope",
                    Type = ScopeType.Resource,
                    Emphasize = true
                },
                new Scope
                {
                    Name = "idmgr",
                    DisplayName = "Identity Manager",
                    Type = ScopeType.Identity,
                    Claims = new List<ScopeClaim>
                    {
                        new ScopeClaim(Constants.ClaimTypes.Name, true),
                        new ScopeClaim(Constants.ClaimTypes.Role, true)
                    }
                }
            };
        }
    }
}