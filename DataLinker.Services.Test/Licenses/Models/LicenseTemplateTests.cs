using System;
using NUnit.Framework;
using DataLinker.Services.Enums;
using DataLinker.Services.LicenseTemplates.Models;

namespace DataLinker.Services.Test.Licenses.Models
{
    [TestFixture]
    public class LicenseTemplateTests
    {
        [Test]
        public void TestLicesneTemplateStatus()
        {
            {
                var licenseTemplate = BuildLicenceTemplate();
                Assert.AreEqual(PublishStatus.Draft, (PublishStatus)licenseTemplate.Status, "License is Draft.");
            }
            {
                var licenseTemplate = BuildLicenceTemplate(PublishStatus.Published);
                Assert.AreEqual(PublishStatus.Published, (PublishStatus)licenseTemplate.Status, "License is Published.");
            }
            {
                var licenseTemplate = BuildLicenceTemplate(PublishStatus.Retracted);
                Assert.AreEqual(PublishStatus.Retracted, (PublishStatus)licenseTemplate.Status, "License is Retracted.");
            }
            {
                var licenseTemplate = BuildLicenceTemplate(PublishStatus.ReadyToPublish);
                Assert.AreEqual(PublishStatus.ReadyToPublish, (PublishStatus)licenseTemplate.Status, "License is Ready to publish.");
            }
        }

        /** Helper test methods **/

        private LicenseTemplate BuildLicenceTemplate(PublishStatus status = PublishStatus.Draft)
        {
            var licenseTemplate = new LicenseTemplate()
            {
                ID = 0,
                Name = "Test License",
                Description = "Test description.",
                CreatedAt = DateTime.Now,
                Status = (int)status,
                CreatedBy = 0
            };

            return licenseTemplate;
        }
    }
}
