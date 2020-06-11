using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Services.Emails.Models.ConsumerProviderRegistrations
{
    public class ConsumerRegistrationDeclineByProviderEmail : CommonEmailProperties
    {
        public string Name;
        public string ProviderOrganizationName;
        public string SchemaName;
        public string DeclineReason;
    }
}
