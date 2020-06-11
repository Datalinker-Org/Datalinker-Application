using System.Collections.Generic;

namespace DataLinker.Models
{
    public class IdentityUser
    {
        public string Id { get; set; }

        public string Username { get; set; }
        public string PlaintextPassword { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Active { get; set; }
        public string PhoneNumber { get; set; }

        public IEnumerable<Claim> Claims { get; set; }
    }
}
