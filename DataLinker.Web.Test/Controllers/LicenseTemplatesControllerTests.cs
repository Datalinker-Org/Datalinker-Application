using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services.Configuration;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseAgreements;
using DataLinker.Services.LicenseAgreements.Models;
using DataLinker.Services.LicenseTemplates;
using DataLinker.Services.LicenseTemplates.Models;
using DataLinker.Services.Organizations;
using DataLinker.Services.Organizations.Models;
using DataLinker.Services.Users;
using DataLinker.Services.Users.Models;
using DataLinker.Web.Controllers;
using DataLinker.Web.Models.Licenses;
using DataLinker.Web.Models.Users;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using PagedList;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class LicenseTemplatesControllerTests
    {
        [SetUp]
        public void Init()
        {
            var today = DateTime.Now;
            organization = new Organization
            {
                ID = 1,
                IsActive = true,
                Name = "Organization",
                TermsOfService = "Terms",
                IsAgreeWithTerms = true
            };

            userOrgLegalOfficer = new User
            {
                ID = 1,
                IsActive = true,
                OrganizationID = organization.ID,
                Name = "LegalOfficer",
                IsSysAdmin = false,
                IsIntroducedAsLegalOfficer = true,
                IsVerifiedAsLegalOfficer = true,
                Email = "Email@mail.com"
            };

            userOrgMember = new User
            {
                ID = 2,
                OrganizationID = organization.ID,
                IsActive = true,
                Email = "Email@mail.com",
                IsSysAdmin = false
            };

            userNotOrgMember = new User
            {
                ID = 3,
                IsActive = true,
                Email = "Email@mail.com",
                IsSysAdmin = false,
                OrganizationID = null
            };

            sysAdmin = new User
            {
                ID = 4,
                IsActive = true,
                Email = "Email@mail.com",
                IsSysAdmin = true,
                OrganizationID = null
            };

            draftLicenseTemplate = new LicenseTemplate
            {
                ID = 1,
                CreatedAt = today,
                CreatedBy = 1,
                Status = (int) TemplateStatus.Draft,
                LicenseText = "<text></text>",
                Name = "titleLicense"
            };

            pendingApprovalLicenseTemplate = new LicenseTemplate
            {
                ID = 20,
                CreatedAt = today,
                CreatedBy = 1,
                Status = (int) TemplateStatus.Active,
                LicenseText = "<text></text>",
                Name = "titleLicense"
            };

            activeLicenseTemplate = new LicenseTemplate
            {
                ID = 3,
                CreatedBy = 1,
                CreatedAt = today,
                Status = (int) TemplateStatus.Active,
                LicenseText = "<text></text>",
                Name = "titleLicense"
            };

            retractedLicenseTemplate = new LicenseTemplate
            {
                ID = 4,
                CreatedBy = 1,
                CreatedAt = today,
                Status = (int) TemplateStatus.Retracted,
                LicenseText = "<text></text>",
                Name = "titleLicense"
            };

            // Init mocks
            licenseService = new Mock<ILicenseTemplatesService>();
            userService = new Mock<IUserService>();
            organizationService = new Mock<IOrganizationService>();
            authentication = new Mock<IAuthenticationManager>();
            configuration = new Mock<IConfigurationService>();
            baseLicenseService = new Mock<ILicenseService>();
            agreementService = new Mock<ILicenseAgreementService>();
            licenseSectionService = new Mock<ILicenseSectionService>();

            baseLicenseService.Setup(i => i.GetAll()).Returns(new List<License> {new License {ID = 1}});
            // Mock Setups:
            configuration.SetupProperty(p => p.ManageLicenseTemplatesPageSize, 5);
            // Organization service
            organizationService.Setup(m => m.Get(organization.ID)).Returns(organization);
            // Setup agreement service
            //agreementService.Setup(m => m.Get(It.IsAny<Expression<Func<LicenseAgreement, bool>>>())).Returns();
            // User service
            userService.Setup(m => m.Get(userNotOrgMember.ID)).Returns(userNotOrgMember);
            userService.Setup(m => m.Get(userOrgMember.ID)).Returns(userOrgMember);
            userService.Setup(m => m.Get(userOrgLegalOfficer.ID)).Returns(userOrgLegalOfficer);
            userService.Setup(m => m.GetLegalOfficers(organization.ID)).Returns(new List<User> {userOrgLegalOfficer});
            // Section service
            licenseSectionService.Setup(m => m.GetAll()).Returns(new List<LicenseSection>());
            // License template service
            licenseService.Setup(m => m.Get(activeLicenseTemplate.ID)).Returns(activeLicenseTemplate);
            licenseService.Setup(m => m.Get(draftLicenseTemplate.ID)).Returns(draftLicenseTemplate);
            licenseService.Setup(m => m.Get(pendingApprovalLicenseTemplate.ID)).Returns(pendingApprovalLicenseTemplate);
            licenseService.Setup(m => m.Get(retractedLicenseTemplate.ID)).Returns(retractedLicenseTemplate);
            licenseService.Setup(m => m.GetAll(false))
                .Returns(new List<LicenseTemplate>
                {
                    activeLicenseTemplate,
                    pendingApprovalLicenseTemplate,
                    draftLicenseTemplate
                });
            licenseService.Setup(m => m.GetAll(true))
                .Returns(new List<LicenseTemplate>
                {
                    retractedLicenseTemplate,
                    activeLicenseTemplate,
                    pendingApprovalLicenseTemplate,
                    draftLicenseTemplate
                });

            authentication.Setup(m => m.SignOut());
            authentication.Setup(m => m.SignIn());
            mockUrlHelper = new Mock<UrlHelper>();
            // Session
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(p => p["isFromConfirmationScreen"]).Returns(true);
            mockSession.SetupGet(p => p["UserWasLoggedIn"]).Returns(true);

            // Setup file
            context = new Mock<ControllerContext>();
            var fileMock = new Mock<HttpPostedFileBase>();
            fileMock.Setup(x => x.FileName).Returns("file1.xml");

            context.Setup(m => m.HttpContext.Request.Files.Count).Returns(1);
            context.Setup(m => m.HttpContext.Request.Files[0]).Returns(fileMock.Object);

            // Controller context
            context.Setup(m => m.HttpContext.Request.Url).Returns(new Uri("http://test.com"));
            context.Setup(m => m.HttpContext.Session).Returns(mockSession.Object);
            controller = new LicenseTemplatesController(licenseService.Object,
                userService.Object, organizationService.Object, licenseSectionService.Object, baseLicenseService.Object, configuration.Object, agreementService.Object, null, null, null,
                null, null)
            {
                LoggedInUser = new LoggedInUserDetails(sysAdmin),
                ControllerContext = context.Object,
                Url = mockUrlHelper.Object
            };
        }

        private LicenseTemplate draftLicenseTemplate;

        private LicenseTemplate activeLicenseTemplate;

        private LicenseTemplate pendingApprovalLicenseTemplate;

        private LicenseTemplate retractedLicenseTemplate;

        private Mock<ILicenseTemplatesService> licenseService;

        private Mock<ILicenseAgreementService> agreementService;

        private Mock<ILicenseSectionService> licenseSectionService;

        private Mock<ILicenseService> baseLicenseService;

        private Mock<IUserService> userService;

        private Mock<IOrganizationService> organizationService;

        private Mock<IAuthenticationManager> authentication;

        private Mock<IConfigurationService> configuration;

        private Mock<ControllerContext> context;

        private LicenseTemplatesController controller;

        private Organization organization;

        private User userOrgLegalOfficer;

        private User userOrgMember;

        private User userNotOrgMember;

        private User sysAdmin;

        private Mock<UrlHelper> mockUrlHelper;

        [Test]
        public void ShouldReturnListOfLicenses()
        {
            var viewResult = (ViewResult) controller.Index(null);
            var model = (IPagedList<LicenseTemplateDetails>) viewResult.Model;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Count == 3);
            Assert.IsTrue(model.Any(p => p.ID == draftLicenseTemplate.ID));
            Assert.IsTrue(model.Any(p => p.ID == pendingApprovalLicenseTemplate.ID));
            Assert.IsFalse(model.Any(p => p.Status == TemplateStatus.Retracted));
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanManageLicenseTemplates()
        {
            controller.LoggedInUser = new LoggedInUserDetails(new User {ID = 9, IsSysAdmin = false});
            controller.Index(null);
        }

        [Test]
        public void ShouldReturnListOfLicensesWithRetracted()
        {
            var viewResult = (ViewResult) controller.Index(null, true);
            var model = (IPagedList<LicenseTemplateDetails>) viewResult.Model;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Count == 4);
            Assert.IsTrue(model.Any(p => p.ID == draftLicenseTemplate.ID));
            Assert.IsTrue(model.Any(p => p.ID == pendingApprovalLicenseTemplate.ID));
            Assert.IsTrue(model.Any(p => p.ID == activeLicenseTemplate.ID));
            Assert.IsTrue(model.Any(p => p.Status == TemplateStatus.Retracted));
        }

        [Test]
        public void ShouldCreateLicensesTemplate()
        {
            userNotOrgMember.IsSysAdmin = true;
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember);
            var viewResult = (ViewResult) controller.Create();
            var model = (LicenseTemplateDetails) viewResult.Model;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Status == TemplateStatus.Draft);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanCreateLicensesTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember);
            controller.Create();
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanViewDetailsForLicensesTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember);
            controller.Details(draftLicenseTemplate.ID);
        }

        [Test]
        public void AdminCanViewDetailsForLicensesTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Details(draftLicenseTemplate.ID);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanSaveChangesForLicensesTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember);
            controller.Save(new LicenseTemplateDetails(draftLicenseTemplate));
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void ShouldFailSaveChangesForRetractedLicensesTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Save(new LicenseTemplateDetails(retractedLicenseTemplate));
            licenseService.Verify(i => i.Get(retractedLicenseTemplate.ID));
        }

        [Test]
        public void ShouldSaveChangesForNewLicensesTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            var model = new LicenseTemplateDetails(activeLicenseTemplate);
            model.ID = null;
            controller.Save(model);
            baseLicenseService.Verify(i=>i.GetAll());
            licenseService.Verify(i=>i.Add(It.IsAny<LicenseTemplate>()));
        }

        [Test]
        public void ShouldSaveChangesForExistingLicensesTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Save(new LicenseTemplateDetails(draftLicenseTemplate));
            licenseService.Verify(i => i.Update(It.IsAny<LicenseTemplate>()));
        }

        [Test]
        public void ShouldReturnEditViewForLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Edit(draftLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(draftLicenseTemplate.ID));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanGetEditViewForLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);
            controller.Edit(draftLicenseTemplate.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanPublishLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);
            controller.Publish(draftLicenseTemplate.ID);
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldReturnErrorForPublishingRetractedLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Publish(retractedLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(retractedLicenseTemplate.ID));
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldReturnErrorForPublishingActiveLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Publish(activeLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(activeLicenseTemplate.ID));
        }
        
        [Test]
        public void ShouldRetractActiveWhenPublishLicenseTemplate()
        {
            licenseService.Setup(m => m.GetPublishedGlobalLicense()).Returns(activeLicenseTemplate);
            var controller = new LicenseTemplatesController(licenseService.Object,
                userService.Object, organizationService.Object, null, baseLicenseService.Object, configuration.Object, null, null, null, null,
                null, null)
            {
                LoggedInUser = new LoggedInUserDetails(sysAdmin),
                ControllerContext = context.Object,
                Url = mockUrlHelper.Object
            };

            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Publish(draftLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(draftLicenseTemplate.ID));
            licenseService.Verify(i => i.GetPublishedGlobalLicense());
            licenseService.Verify(i => i.Update(It.IsAny<LicenseTemplate>()));
        }

        [Test]
        public void ShouldPublishDraftLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Publish(draftLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(draftLicenseTemplate.ID));
            licenseService.Verify(i => i.GetPublishedGlobalLicense());
            licenseService.Verify(i=>i.Update(It.IsAny<LicenseTemplate>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanRetractDraftLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);
            controller.Retract(draftLicenseTemplate.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailRetractRetractedLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Retract(retractedLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(retractedLicenseTemplate.ID));
        }

        [Test]
        public void ShouldRetractDraftLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Retract(draftLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(draftLicenseTemplate.ID));
        }

        [Test]
        public void ShouldRetractActiveLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Retract(activeLicenseTemplate.ID);
            licenseService.Verify(i => i.Get(activeLicenseTemplate.ID));
            licenseService.Verify(i => i.Update(It.IsAny<LicenseTemplate>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailDownloadLicenseTemplateForInvalidInput()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);
            controller.Download(null);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailDownloadLicenseTemplateForNotExistingTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);
            controller.Download(0);
        }

        [Test]
        public void ShouldDownloadLicenseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Download(retractedLicenseTemplate.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGenerateReportForLicenseAgreements()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);
            controller.GenerateReport();
        }

        [Test]
        public void ShouldGenerateReportForLicenseAgreements()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.GenerateReport();
            agreementService.Verify(i=>i.Get(It.IsAny<Expression<Func<LicenseAgreement, bool>>>()));
            licenseSectionService.Verify(i=>i.GetAll());
        }
    }
}