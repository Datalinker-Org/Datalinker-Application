using DataLinker.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Models.Applications
{
    public class ProviderModel
    {
        public int SchemaId;
        public string SchemaName;
        public int ApplicationId;
        public string ApplicationName;
        public int LicenseId;
        public ConsumerProviderRegistrationStatus Status;
    }
}
