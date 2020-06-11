using System.ComponentModel;

namespace DataLinker.Models.Enums
{
    public enum ActivationState
    {
        [Description("not active")]
        NotActive = 0,
        [Description("active")]
        Active = 1
    }
}