using System;
using System.ComponentModel.DataAnnotations;
using DataLinker.Models.Enums;

namespace DataLinker.Models
{
    public class ProviderLicenseModel
    {
        public int ID { get; set; }

        public int CustomLicenseId { get; set; }

        public PublishStatus Status { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        public bool IsCustom => CustomLicenseId != 0;

        public string OrgName { get; set; }

        public string TemplateName { get; set; }

        public bool IsReadyToPublish => Status == PublishStatus.ReadyToPublish;

        public bool IsPendingApproval => Status == PublishStatus.PendingApproval;

        public bool IsPublished => Status == PublishStatus.Published;

        public bool IsDraft => Status == PublishStatus.Draft;

        public bool IsRetracted => Status == PublishStatus.Retracted;
    }
}