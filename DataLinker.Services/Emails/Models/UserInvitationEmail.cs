using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Services.Emails.Models
{
    public class UserInvitationEmail : CommonEmailProperties
    {
        public string Name { get; set; }
        public string VerificationLink { get; set; }
        public string OrgName { get; set; }
        public string InviterName { get; set; }
    }
}