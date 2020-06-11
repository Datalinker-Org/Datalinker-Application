using System;

namespace DataLinker.Models.Enums
{
    /// <summary>
    /// Indication of the publish state of an item.
    /// </summary>
    public enum PublishStatus : int
    {
        Retracted = 1,
        Draft = 2,
        PendingApproval = 3,
        ReadyToPublish = 4,
        Published = 5
    }

    public static class PublishStatusExtensions
    {
        public static string ToStringWithSpaces(this PublishStatus value)
        {
            switch (value)
            {
                case PublishStatus.Retracted:
                case PublishStatus.Draft:
                case PublishStatus.Published:
                    return value.ToString();
                case PublishStatus.PendingApproval:
                    return "Pending Approval";
                case PublishStatus.ReadyToPublish:
                    return "Ready To Publish";
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}
