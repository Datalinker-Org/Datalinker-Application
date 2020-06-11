using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Services.Emails.Models.ConsumerProviderRegistrations
{
    public class ProviderLegalApproveEmail : CommonEmailProperties
    {
        public string Name { get; set; }
        public string ConsumerOrganizationName { get; set; }
        public string ProviderOrganizationName { get; set; }
        public string SchemaName { get; set; }
    }
}
