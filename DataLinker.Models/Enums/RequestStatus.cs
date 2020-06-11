using System.ComponentModel;

namespace DataLinker.Models.Enums
{
    public enum RequestStatus
    {
        [Description("Not Processed")]
        NotProcessed = 0,
        [Description("Approved")]
        Approved = 1,
        [Description("Declined")]
        Declined = 2
    }
}