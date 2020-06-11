using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataLinker.Models.Enums
{
    public enum ConsumerProviderRegistrationStatus
    {
        NotRegistered = 0,
        PendingConsumerApproval = 1,
        PendingProviderApproval = 2,
        Approved = 3,
        Declined = 4
    }
}
