using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using DataLinker.Services.Applications;
using DataLinker.Services.Applications.Models;
using DataLinker.Services.Configuration;
using DataLinker.Services.Emails;
using DataLinker.Services.Emails.Roles.Admin;
using DataLinker.Services.Emails.Roles.LegalOfficer;
using DataLinker.Services.Emails.Roles.User;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseAgreements;
using DataLinker.Services.LicenseContent;
using DataLinker.Services.LicenseTemplates;
using DataLinker.Services.LicenseTemplates.Models;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Services.OrganizationLicenses.Models;
using DataLinker.Services.Organizations;
using DataLinker.Services.Organizations.Models;
using DataLinker.Services.ProviderEndpoints;
using DataLinker.Services.ProviderEndpoints.Models;
using DataLinker.Services.Schemas;
using DataLinker.Services.Schemas.Models;
using DataLinker.Services.SoftwareStatements;
using DataLinker.Services.Users;
using DataLinker.Services.Users.Models;
using DataLinker.Web.Controllers;
using DataLinker.Web.Models.Licenses;
using DataLinker.Web.Models.Users;
using Moq;
using NUnit.Framework;
using DataLinker.Models.OrganisationLicenses;
using Newtonsoft.Json;
using DataLinker.Services.Tests;
using DataLinker.Services.LicenseMatches.Models;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class LicensesControllerTests
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

            otherOrganization = new Organization
            {
                ID = 2,
                IsActive = true,
                Name = "Organization2",
                TermsOfService = "Terms",
                IsAgreeWithTerms = true
            };

            sysAdmin = new User
            {
                ID = 1,
                IsActive = true,
                Name = "Admin",
                IsSysAdmin = true,
                IsIntroducedAsLegalOfficer = false,
                IsVerifiedAsLegalOfficer = false,
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
                OrganizationID = otherOrganization.ID
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

            dataSchema = new DataSchema
            {
                ID = 1,
                Name = "Schemam",
                IsIndustryGood = false,
                CreatedBy = userOrgMember.ID,
                CreatedAt = DateTime.Now
            };

            application = new Application
            {
                ID = 1,
                IsActive = true,
                OrganizationID = organization.ID,
                IsProvider = true
            };

            consumerApplication = new Application
            {
                ID = 1,
                IsActive = true,
                OrganizationID = organization.ID,
                IsProvider = false
            };

            endpoint = new ProviderEndpoint
            {
                ApplicationId = application.ID,
                DataSchemaID = dataSchema.ID,
                ID = 1,
                IsIndustryGood = true,
                Name = "Endpoint"
            };

            activeLicenseTemplate = new LicenseTemplate
            {
                ID = 2,
                CreatedBy = 1,
                CreatedAt = today,
                LicenseText = "<test></test>",
                Status = (int)TemplateStatus.Active
            };

            draftLicense = new OrganizationLicense
            {
                ID = 1,
                CreatedAt = today,
                CreatedBy = 1,
                Status = (int)PublishStatus.Draft,
                DataSchemaID = dataSchema.ID,
                LicenseTemplateID = activeLicenseTemplate.ID
            };

            approvedLicense = new OrganizationLicense
            {
                ID = 2,
                CreatedBy = 1,
                CreatedAt = today,
                Status = (int)PublishStatus.ReadyToPublish,
                ApprovedAt = today,
                ApprovedBy = 1
            };

            pendingApprovalLicense = new OrganizationLicense
            {
                ID = 20,
                CreatedAt = today,
                CreatedBy = 1,
                Status = (int)PublishStatus.PendingApproval,
                DataSchemaID = dataSchema.ID,
                LicenseTemplateID = activeLicenseTemplate.ID
            };

            licenseTemplate = new LicenseTemplate
            {
                ID = 1,
                LicenseID = 1,
                Status = (int)TemplateStatus.Active
            };

            license = new OrganizationLicense
            {
                ID = 3,
                Status = (int)PublishStatus.Published,
                DataSchemaID = dataSchema.ID,
                LicenseTemplateID = licenseTemplate.ID,
                ProviderEndpointID = endpoint.ID,
                CreatedBy = userOrgMember.ID,
                CreatedAt = DateTime.Now
            };

            sectionAttribution = new LicenseSection
            {
                ID = 1,
                Title = "Attribution"
            };

            sectionPayment = new LicenseSection
            {
                ID = 2,
                Title = "Payment"
            };

            attrClause = new LicenseClause
            {
                ID = 1,
                LicenseSectionID = sectionAttribution.ID
            };

            paymentClause = new LicenseClause
            {
                ID = 2,
                LicenseSectionID = sectionPayment.ID
            };

            attrClauseTemplate = new LicenseClauseTemplate
            {
                ID = 1,
                LicenseClauseID = attrClause.ID,
                Status = (int)TemplateStatus.Active,
                ClauseType = (int)ClauseType.Text
            };

            paymentClauseTemplate = new LicenseClauseTemplate
            {
                ID = 2,
                LicenseClauseID = paymentClause.ID,
                Status = (int)TemplateStatus.Active,
                ClauseType = (int)ClauseType.Text
            };

            // Init mocks
            licenseTemplateService = new Mock<ILicenseTemplatesService>();
            userService = new Mock<IUserService>();
            organizationService = new Mock<IOrganizationService>();
            licenseService = new Mock<IOrganizationLicenseService>();
            endpointService = new Mock<IProviderEndpointService>();
            dataSchemaService = new Mock<IDataSchemaService>();
            applicationService = new Mock<IApplicationsService>();
            notificationService = new Mock<INotificationService>();
            configuration = new Mock<IConfigurationService>();
            applicationTokenService = new Mock<IApplicationTokenService>();
            requestService = new Mock<ILicenseApprovalRequestService>();
            licenseClauseServise = new Mock<IOrganizationLicenseClauseService>();
            sectionService = new Mock<ILicenseSectionService>();
            licenseContentBuilder = new Mock<ILicenseContentBuilder>();
            clauseService = new Mock<ILicenseClauseService>();
            clauseTemplateService = new Mock<ILicenseClauseTemplateService>();
            agreeementService = new Mock<ILicenseAgreementService>();
            schemaFileService = new Mock<ISchemaFileService>();
            userNotificationService = new Mock<IUserNotificationService>();
            legalOfficerNotificationService = new Mock<ILegalOfficerNotificationService>();
            adminNotificationService = new Mock<IAdminNotificationService>();
            softwareStatementService = new Mock<ISoftwareStatementService>();
            userInputService = new MockService<UserInput>();
            var selections = new List<SectionsWithClauses> {
                new SectionsWithClauses
                {
                    Clauses = new List<ClauseModel>
                    {
                        new ClauseModel
                        {
                            ClauseId = 1,
                            LegalText = "some text"
                        }
                    }
                }
            };
            var serializedInput = JsonConvert.SerializeObject(selections);
            userInputService.Add(new UserInput { UserID = userOrgMember.ID, DataSchemaId = dataSchema.ID, LicenseSelection = serializedInput });

            // Setup notification service
            notificationService.SetupGet(i => i.User).Returns(userNotificationService.Object);
            notificationService.SetupGet(i => i.LegalOfficer).Returns(legalOfficerNotificationService.Object);
            notificationService.SetupGet(i => i.Admin).Returns(adminNotificationService.Object);

            licenseRequest = new LicenseApprovalRequest { SentTo = userOrgLegalOfficer.ID };
            // Mock Setups:
            // Configuration
            configuration.SetupGet(m => m.ApprovalLinkExpiresIn).Returns("1");
            configuration.SetupProperty(m => m.ManageLicensesPageSize, 5);
            // Schema file
            schemaFileService.Setup(i => i.Get(It.IsAny<Expression<Func<SchemaFile, bool>>>()))
                .Returns(new List<SchemaFile> { new SchemaFile { ID = dataSchema.ID } });
            // Endpoints
            endpointService.Setup(p => p.Get(It.IsAny<Expression<Func<ProviderEndpoint, bool>>>()))
                .Returns(new List<ProviderEndpoint> { endpoint });
            endpointService.Setup(m => m.Get(endpoint.ID)).Returns(endpoint);

            // Applications
            applicationService.Setup(m => m.Get(application.ID)).Returns(application);
            applicationService.Setup(m => m.Get(consumerApplication.ID)).Returns(consumerApplication);
            applicationService.Setup(p => p.GetAuthenticationFor(application.ID)).Returns(new ApplicationAuthentication());

            // Schemas
            dataSchemaService.Setup(m => m.Get(dataSchema.ID)).Returns(dataSchema);

            // Organization service
            organizationService.Setup(m => m.Get(organization.ID)).Returns(organization);

            // License service
            licenseService.Setup(m => m.Get(license.ID)).Returns(license);
            licenseService.Setup(m => m.Get(It.IsAny<Expression<Func<OrganizationLicense, bool>>>())).Returns(new List<OrganizationLicense> { license });
            licenseService.Setup(m => m.GetPublishedProviderLicenses(dataSchema.ID, license.LicenseTemplateID)).Returns(new List<OrganizationLicense> { license });
            licenseService.Setup(m => m.GetPublishedForEndpoint(endpoint.ID)).Returns(new List<OrganizationLicense> { license });
            licenseService.Setup(m => m.Get(approvedLicense.ID)).Returns(approvedLicense);
            licenseService.Setup(m => m.Get(draftLicense.ID)).Returns(draftLicense);
            licenseService.Setup(m => m.Get(pendingApprovalLicense.ID)).Returns(pendingApprovalLicense);
            licenseContentBuilder.Setup(m => m.GetLicenseContent(It.IsAny<int>())).Returns(new XmlDocument());
            requestService.Setup(i => i.Get(It.IsAny<Expression<Func<LicenseApprovalRequest, bool>>>()))
                .Returns(new List<LicenseApprovalRequest> { licenseRequest });

            // License template service
            licenseTemplateService.Setup(m => m.Get(activeLicenseTemplate.ID)).Returns(activeLicenseTemplate);

            licenseTemplateService.Setup(p => p.GetAll(false)).Returns(new List<LicenseTemplate> { licenseTemplate });
            licenseTemplateService.Setup(p => p.GetPublishedGlobalLicense()).Returns(licenseTemplate);
            licenseTemplateService.Setup(p => p.Get(licenseTemplate.ID)).Returns(licenseTemplate);

            // Section service
            sectionService.Setup(p => p.GetAll()).Returns(new List<LicenseSection> { sectionAttribution, sectionPayment });
            sectionService.Setup(p => p.Get(sectionAttribution.ID)).Returns(sectionAttribution);
            sectionService.Setup(p => p.Get(sectionPayment.ID)).Returns(sectionPayment);

            // clause service
            clauseService.Setup(p => p.Get(It.IsAny<Expression<Func<LicenseClause, bool>>>()))
                .Returns(new List<LicenseClause> { paymentClause, attrClause });
            clauseService.Setup(i => i.Get(paymentClause.ID)).Returns(paymentClause);
            clauseService.Setup(i => i.Get(attrClause.ID)).Returns(attrClause);

            // clause template
            clauseTemplateService.Setup(p => p.Get(It.IsAny<Expression<Func<LicenseClauseTemplate, bool>>>()))
                .Returns(new List<LicenseClauseTemplate> { paymentClauseTemplate, attrClauseTemplate });

            clauseTemplateService.Setup(p => p.GetTemplatesForClause(paymentClause.ID, TemplateStatus.Active))
                .Returns(new List<LicenseClauseTemplate> { paymentClauseTemplate });

            clauseTemplateService.Setup(p => p.GetTemplatesForClause(attrClause.ID, TemplateStatus.Active))
                .Returns(new List<LicenseClauseTemplate> { attrClauseTemplate });

            // User service
            userService.Setup(m => m.Get(userNotOrgMember.ID)).Returns(userNotOrgMember);
            userService.Setup(m => m.Get(userOrgMember.ID)).Returns(userOrgMember);
            userService.Setup(m => m.Get(sysAdmin.ID)).Returns(sysAdmin);
            userService.Setup(m => m.GetSystemAdministrators()).Returns(new List<User> { sysAdmin });
            userService.Setup(m => m.Get(userOrgLegalOfficer.ID)).Returns(userOrgLegalOfficer);
            userService.Setup(m => m.GetLegalOfficers(organization.ID)).Returns(new List<User> { userOrgLegalOfficer });

            context = new Mock<ControllerContext>();
            var mock = new Mock<UrlHelper>();

            var mService = new MockService<LicenseMatch>();
            matchesService = new LicenseMatchesService(mService);

            // Session
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(p => p["UserWasLoggedIn"]).Returns(true);

            // Controller context
            context.Setup(m => m.HttpContext.Request.Form).Returns(new FormCollection());
            context.Setup(m => m.HttpContext.Request.Url).Returns(new Uri("http://test.com"));
            context.Setup(m => m.HttpContext.Request.Browser).Returns(new Mock<HttpBrowserCapabilitiesBase>().Object);
            context.Setup(m => m.HttpContext.Session).Returns(mockSession.Object);
            controller = new LicensesController(userService.Object,
                organizationService.Object, licenseService.Object,
                endpointService.Object, dataSchemaService.Object,
                licenseTemplateService.Object, applicationService.Object,
                notificationService.Object, configuration.Object,
                requestService.Object, licenseContentBuilder.Object,
                licenseClauseServise.Object, clauseTemplateService.Object,
                clauseService.Object, sectionService.Object, agreeementService.Object,
                schemaFileService.Object, softwareStatementService.Object,
                applicationTokenService.Object, userInputService, matchesService)
            {
                ControllerContext = context.Object,
                Url = mock.Object
            };
        }

        private LicenseTemplate activeLicenseTemplate;

        private LicenseApprovalRequest licenseRequest;

        private OrganizationLicense draftLicense;

        private OrganizationLicense approvedLicense;

        private OrganizationLicense pendingApprovalLicense;

        private ProviderEndpoint endpoint;

        private OrganizationLicense license;

        private Application application;

        private Application consumerApplication;

        private DataSchema dataSchema;
        private LicenseTemplate licenseTemplate;
        private LicenseSection sectionAttribution;
        private LicenseSection sectionPayment;
        private LicenseClause attrClause;
        private LicenseClause paymentClause;
        private LicenseClauseTemplate attrClauseTemplate;
        private LicenseClauseTemplate paymentClauseTemplate;

        private ILicenseMatchesService matchesService;

        private Mock<ILicenseTemplatesService> licenseTemplateService;

        private Mock<ISoftwareStatementService> softwareStatementService;

        private Mock<ILicenseClauseService> clauseService;

        private Mock<ILicenseContentBuilder> licenseContentBuilder;

        private Mock<ILicenseApprovalRequestService> requestService;

        private Mock<ILicenseClauseTemplateService> clauseTemplateService;

        private Mock<ILicenseAgreementService> agreeementService;

        private Mock<IOrganizationLicenseClauseService> licenseClauseServise;

        private Mock<ILicenseSectionService> sectionService;

        private Mock<IUserService> userService;

        private Mock<IOrganizationService> organizationService;

        private Mock<IOrganizationLicenseService> licenseService;

        private Mock<IProviderEndpointService> endpointService;

        private Mock<IDataSchemaService> dataSchemaService;

        private Mock<ISchemaFileService> schemaFileService;

        private Mock<IApplicationsService> applicationService;

        private Mock<IApplicationTokenService> applicationTokenService;

        private Mock<IConfigurationService> configuration;

        private Mock<INotificationService> notificationService;

        private Mock<IUserNotificationService> userNotificationService;

        private Mock<ILegalOfficerNotificationService> legalOfficerNotificationService;

        private Mock<IAdminNotificationService> adminNotificationService;

        private MockService<UserInput> userInputService;

        private Mock<ControllerContext> context;

        private LicensesController controller;

        private Organization organization;

        private Organization otherOrganization;

        private User userOrgLegalOfficer;

        private User sysAdmin;

        private User userOrgMember;

        private User userNotOrgMember;

        [Test]
        public void ShoudReturnListOfLicensesForEndpoint()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (ViewResult)controller.Index(endpoint.ID, null);
            var model = (IEnumerable<ProviderLicenseModel>)result.Model;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Any());
        }

        [Test]
        public void ShoudReturnListOfLicensesForEndpointForAdmin()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (ViewResult)controller.Index(endpoint.ID, null);
            var model = (IEnumerable<ProviderLicenseModel>)result.Model;
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Any());
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShoudNotReturnListOfLicensesForEndpointToUserOfAnotherOrg()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.Index(endpoint.ID, null);
        }

        [Test]
        public void ShouldPublishLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Publish(approvedLicense.ID, application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowNotOrgMemberPublishLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.Publish(license.ID, application.ID, endpoint.ID);
        }

        [Test]
        public void ShouldAllowAdminToPublishLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.Publish(approvedLicense.ID, application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowToPublishPublishedLicense()
        {
            license.Status = (int) PublishStatus.Published;
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Publish(license.ID, application.ID, endpoint.ID);
        }

        [Test]
        public void ShouldRetractLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Retract(license.ID, application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowNotOrgMemberRetractLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.Retract(license.ID, application.ID);
        }

        [Test]
        public void ShouldAllowAdminToRetractLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Retract(license.ID, application.ID);
            licenseService.Verify(i=>i.Update(It.IsAny<OrganizationLicense>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowToRetractDraftLicense()
        {
            license.Status = (int) PublishStatus.Retracted;
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.Retract(license.ID, application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShoudNotAllowProcessInConfirmScreenAlreadyApprovedLicense()
        {
            pendingApprovalLicense.ApprovedAt = DateTime.Now;
            pendingApprovalLicense.ApprovedBy = 2;
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var linkToken = "FAAAAAMAAAAA/iWFOCTTiHUar9GrbkhKg5zL+UCHaZA=";
            licenseRequest.Token = "d1af1a75-6eab-4a48-839c-cbf940876990";
            licenseRequest.ExpiresAt = DateTime.Parse("2016-01-24 10:02:36");
            controller.ConfirmationScreen(linkToken);
        }

        [Test]
        public void ShouldApprovePendingApprovalLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (ViewResult)controller.Approve(pendingApprovalLicense.ID);
            Assert.IsTrue(result.ViewName == "LicenseVerificationThankYou");
        }

        [Test]
        public void ShouldCallNotifyLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            { Organization = new LoggedInOrganization(organization) };
            controller.SendToLegalOfficer(draftLicense.ID, endpoint.ID);
            legalOfficerNotificationService.Verify(m => m.LicenseIsPendingApprovalInBackground(userOrgLegalOfficer.ID, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), draftLicense.ID, It.IsAny<bool>()));
            
        }

        [Test]
        public void ShouldDeclinePendingApprovalLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (ViewResult)controller.Decline(pendingApprovalLicense.ID);
            Assert.IsTrue(result.ViewName == "LicenseVerificationThankYou");
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowAccessPageInConfirmScreenForNotLegalOfficers()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var linkToken = "FAAAAAMAAAAA/iWFOCTTiHUar9GrbkhKg5zL+UCHaZA=";
            licenseRequest.Token = "d1af1a75-6eab-4a48-839c-cbf940876990";
            licenseRequest.ExpiresAt = DateTime.Parse("2016-01-24 10:02:36");
            controller.ConfirmationScreen(linkToken);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowAccessPageInConfirmScreenIfUserNotLegalOfficerInHisCompany()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var linkToken = "FAAAAAMAAAAA/iWFOCTTiHUar9GrbkhKg5zL+UCHaZA=";
            licenseRequest.Token = "d1af1a75-6eab-4a48-839c-cbf940876990";
            licenseRequest.ExpiresAt = DateTime.Parse("2016-01-24 10:02:36");
            controller.ConfirmationScreen(linkToken);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowUserWithoutOrganizationSendLicenseToLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);

            controller.SendToLegalOfficer(draftLicense.ID, endpoint.ID);
        }

        [Test]
        public void ShouldNotifyUserAboutLicenseApproval()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Approve(pendingApprovalLicense.ID);
            userNotificationService.Verify(
                m =>
                    m.StatusForLicenseUpdatedInBackground(licenseRequest.SentBy,
                        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(),(int)PublishStatus.ReadyToPublish));
        }

        [Test]
        public void ShouldNotifyUserAboutLicenseDecline()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var reciever = licenseRequest.SentBy;
            controller.Decline(pendingApprovalLicense.ID);
            userNotificationService.Verify(
                m =>
                    m.StatusForLicenseUpdatedInBackground(reciever, It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>(), It.IsAny<bool>(), (int)PublishStatus.Draft));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventApproveAlreadyApprovedLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Approve(approvedLicense.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventApproveNotPendingApprovalLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Approve(draftLicense.ID);
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventApproveProcessingIfUserIsNotLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember);
            controller.Approve(pendingApprovalLicense.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventDeclineAlreadyApprovedLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Decline(approvedLicense.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventDeclineNotPendingApprovalLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Decline(draftLicense.ID);
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventDeclineProcessingIfUserIsNotLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember);
            controller.Decline(pendingApprovalLicense.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventProcessingLicenseInConfirmScreenWhichIsNotPendingApproval()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var linkToken = "FAAAAAMAAAAA/iWFOCTTiHUar9GrbkhKg5zL+UCHaZA=";
            licenseRequest.Token = "d1af1a75-6eab-4a48-839c-cbf940876990";
            licenseRequest.ExpiresAt = DateTime.Parse("2016-01-24 10:02:36");
            controller.ConfirmationScreen(linkToken);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventSendingNotDraftLicenseToLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.SendToLegalOfficer(pendingApprovalLicense.ID, endpoint.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldRollBackLicenseToDraftAnyExceptionOccurs()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var linkToken = "FAAAAAMAAAAA/iWFOCTTiHUar9GrbkhKg5zL+UCHaZA=";
            licenseRequest.Token = "d1af1a75-6eab-4a48-839c-cbf940876990";
            licenseRequest.ExpiresAt = DateTime.Parse("2016-01-24 10:02:36");
            controller.ConfirmationScreen(linkToken);
            licenseService.Verify(m => m.Update(pendingApprovalLicense));
            Assert.IsTrue(pendingApprovalLicense.IsDraft);
        }

        [Test]
        public void ShouldSendDraftLicenseToLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            { Organization = new LoggedInOrganization(organization) };
            controller.SendToLegalOfficer(draftLicense.ID, endpoint.ID);

            // Verification
            licenseService.Verify(m => m.Get(draftLicense.ID));
            licenseService.Verify(m => m.Update(draftLicense));
            userService.Verify(m => m.GetLegalOfficers(userOrgMember.OrganizationID.Value));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldUpdateLicenseInConfirmScreenWhenApprovalLinkIsExpired()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgLegalOfficer)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var linkToken = "FAAAAAMAAAAA/iWFOCTTiHUar9GrbkhKg5zL+UCHaZA=";
            licenseRequest.Token = "d1af1a75-6eab-4a48-839c-cbf940876990";
            licenseRequest.ExpiresAt = DateTime.Now.AddDays(-1);
            controller.ConfirmationScreen(linkToken);
            licenseService.Verify(m => m.Update(pendingApprovalLicense));
            Assert.IsTrue(pendingApprovalLicense.IsDraft);
        }

        [Test]
        public void ShouldMoveLicenseToDraft()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.MoveToDraft(pendingApprovalLicense.ID, application.ID, endpoint.ID);
            licenseService.Verify(i=>i.Update(pendingApprovalLicense));
            Assert.IsTrue(pendingApprovalLicense.IsDraft);
        }

        [Test]
        public void ShouldAllowAdminMoveLicenseToDraft()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.MoveToDraft(pendingApprovalLicense.ID, application.ID, endpoint.ID);
            Assert.IsTrue(pendingApprovalLicense.IsDraft);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailMoveLicenseToDraftForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.MoveToDraft(pendingApprovalLicense.ID, application.ID, endpoint.ID);
            Assert.IsFalse(pendingApprovalLicense.IsDraft);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailMoveLicenseToDraftForNotOrgMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.MoveToDraft(pendingApprovalLicense.ID, application.ID, endpoint.ID);
            Assert.IsFalse(pendingApprovalLicense.IsDraft);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailMoveDraftLicenseToDraft()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.MoveToDraft(draftLicense.ID, application.ID, endpoint.ID);
            Assert.IsFalse(pendingApprovalLicense.IsDraft);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailMovePublishedLicenseToDraft()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.MoveToDraft(draftLicense.ID, application.ID, endpoint.ID);
            Assert.IsFalse(pendingApprovalLicense.IsDraft);
        }

        [Test]
        public void ShouldDownloadLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Download(pendingApprovalLicense.ID, application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailDownloadIfNoGlobaLicense()
        {
            pendingApprovalLicense.LicenseTemplateID = 0;
            controller = new LicensesController(userService.Object, organizationService.Object, licenseService.Object,
                endpointService.Object, dataSchemaService.Object, licenseTemplateService.Object,
                applicationService.Object, notificationService.Object, configuration.Object, 
                requestService.Object,licenseContentBuilder.Object,
                licenseClauseServise.Object, clauseTemplateService.Object,
                clauseService.Object, sectionService.Object,agreeementService.Object,
                schemaFileService.Object,softwareStatementService.Object,applicationTokenService.Object,
                userInputService, matchesService);
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Download(pendingApprovalLicense.ID, application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowNotActiveOrgDownloadLicense()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Download(pendingApprovalLicense.ID, application.ID);
        }

        [Test]
        public void ShouldAllowNotOrgMemberDownloadLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.Download(pendingApprovalLicense.ID, application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowUserWithoutOrgDownloadLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember);

            controller.Download(pendingApprovalLicense.ID, application.ID);
        }

        [Test]
        public void ShouldReturnModelToBuildProviderLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result =
                (ViewResult)
                    controller.BuildLicense(endpoint.ApplicationId, endpoint.DataSchemaID,
                        endpoint.ID);
            var model = (BuildLicenseModel)result.Model;
            Assert.IsTrue(model.Sections.Count > 0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventNotMemberToBuildLicense()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.BuildLicense(endpoint.ApplicationId, endpoint.DataSchemaID, endpoint.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotReturnModelToBuildLicenseForNotMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };
            var model = new BuildLicenseModel
            {
                Sections = new List<SectionsWithClauses> { new SectionsWithClauses { ApplicationId = application.ID } }
            };
            controller.BuildLicense(endpoint.ID, model);
        }

        [Test]
        public void ShouldPerformBuildLicenseForConsumer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var view = (ViewResult)controller.BuildLicense(consumerApplication.ID, endpoint.DataSchemaID);
            var model = (BuildLicenseModel)view.Model;
            Assert.IsTrue(model.Sections.Count > 0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotPerformBuildLicenseIfGlobalLicenseDoesNotPublished()
        {
            licenseTemplateService.Setup(m => m.GetPublishedGlobalLicense()).Returns((LicenseTemplate)null);

            controller = new LicensesController(userService.Object, organizationService.Object, licenseService.Object,
                endpointService.Object, dataSchemaService.Object, licenseTemplateService.Object,
                applicationService.Object, notificationService.Object, configuration.Object, requestService.Object,
                licenseContentBuilder.Object, licenseClauseServise.Object, clauseTemplateService.Object, clauseService.Object,
                sectionService.Object,agreeementService.Object, schemaFileService.Object, softwareStatementService.Object,
                applicationTokenService.Object, userInputService, matchesService);

            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.BuildLicense(consumerApplication.ID, endpoint.DataSchemaID);
        }

        [Test]
        public void ShouldReturnMatchesViewToConsumer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var view = (ViewResult)controller.BuildLicense(consumerApplication.ID, endpoint.DataSchemaID);
            var model = (BuildLicenseModel)view.Model;

            // Select all clauses in model
            var id = 0;
            foreach (var item in model.Sections)
            {
                foreach (var clause in item.Clauses)
                {
                    clause.IsSelectedByConsumer = true;
                    clause.ClauseId = ++id;
                }
            }
            var result = (RedirectToRouteResult)controller.BuildLicense(dataSchema.ID, model);
            Assert.IsTrue(result.RouteValues["action"].ToString() == "ProviderMatches");
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailReturnProviderMatchesForNotActiveOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.LoggedInUser.Organization.IsActive = false;
            var model = new BuildLicenseModel
            {
                Sections = new List<SectionsWithClauses>()
            };

            controller.BuildLicense(endpoint.DataSchemaID, model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailReturnProviderMatchesIfGlobalLicenseDoesNotPublished()
        {
            licenseTemplateService.Setup(m => m.GetPublishedGlobalLicense()).Returns((LicenseTemplate)null);

            controller = new LicensesController(userService.Object, organizationService.Object, licenseService.Object,
                endpointService.Object, dataSchemaService.Object, licenseTemplateService.Object,
                applicationService.Object, notificationService.Object, configuration.Object, requestService.Object,
                licenseContentBuilder.Object, licenseClauseServise.Object, clauseTemplateService.Object,
                clauseService.Object, sectionService.Object, agreeementService.Object, schemaFileService.Object,
                softwareStatementService.Object, applicationTokenService.Object, userInputService, matchesService);

            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var view = (ViewResult)controller.BuildLicense(consumerApplication.ID, endpoint.DataSchemaID);
            var model = (List<SectionsWithClauses>)view.Model;
            var vModel = new BuildLicenseModel { Sections = model };
            controller.BuildLicense(endpoint.DataSchemaID, vModel);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailSaveProviderMatchesForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.ProviderMatches(application.ID, dataSchema.ID, new ProviderComparisonSummary());
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailDisplayProviderMatchesForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.ProviderMatches(application.ID, dataSchema.ID);
        }

        [Test]
        public void ShouldDisplayProviderMatchesForActiveOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };
            // TODO: user services setup from SNPSHot instead of moq
            controller.ProviderMatches(application.ID, dataSchema.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailSaveProviderMatchesForOrganizationMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.ProviderMatches(application.ID, dataSchema.ID, new ProviderComparisonSummary());
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailSaveProviderMatchesIfNothingToSave()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ProviderComparisonSummary { Endpoints = new List<ProviderMatch> { new ProviderMatch { IsMatch = false } } };
            controller.ProviderMatches(application.ID, dataSchema.ID, model);
            licenseService.Verify(i => i.Add(It.IsAny<OrganizationLicense>()));
        }

        [Test]
        public void ShouldFailSaveProviderMatchesIfApplicationNotExists()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (HttpStatusCodeResult)controller.ProviderMatches(0, dataSchema.ID, new ProviderComparisonSummary());
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailSaveProviderMatchesIfGlobalLicenseNotFound()
        {
            licenseTemplateService.Setup(m => m.GetPublishedGlobalLicense()).Returns((LicenseTemplate)null);

            controller = new LicensesController(userService.Object, organizationService.Object, licenseService.Object,
                endpointService.Object, dataSchemaService.Object, licenseTemplateService.Object,
                applicationService.Object, notificationService.Object, configuration.Object, requestService.Object,
                licenseContentBuilder.Object, licenseClauseServise.Object, clauseTemplateService.Object, clauseService.Object,
                sectionService.Object,agreeementService.Object, schemaFileService.Object, softwareStatementService.Object, applicationTokenService.Object, userInputService)
            {
                LoggedInUser = new LoggedInUserDetails(userOrgMember)
                {
                    Organization = new LoggedInOrganization(organization)
                }
            };

            controller.ProviderMatches(application.ID, dataSchema.ID, new ProviderComparisonSummary());
        }

        [Test]
        public void ShouldSaveProviderMatches()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.TempData = new TempDataDictionary {{"ConsumerChoices", new List<SectionsWithClauses>()}};
            var model = new ProviderComparisonSummary
            {
                Endpoints =
                    new List<ProviderMatch>
                    {
                        new ProviderMatch {IsMatch = true, Clauses = new List<ClauseMatch>()}
                    }
            };

            controller.ProviderMatches(application.ID, dataSchema.ID, model);
            licenseService.Verify(i => i.Add(It.IsAny<OrganizationLicense>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetConsumerLicensesForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.ConsumerLicenses(consumerApplication.ID, dataSchema.ID);
        }

        [Test]
        public void ShouldFailGetConsumerLicensesForNotExistingApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (HttpStatusCodeResult)controller.ConsumerLicenses(0, dataSchema.ID);
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetConsumerLicensesForNotOrganizationMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userOrgMember)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.ConsumerLicenses(consumerApplication.ID, dataSchema.ID);
        }

        [Test]
        public void ShouldGetConsumerLicenses()
        {
            controller.LoggedInUser = userOrgMember.ToModel();
            controller.LoggedInUser.Organization = organization.ToModel();

            controller.ConsumerLicenses(consumerApplication.ID, dataSchema.ID);
            licenseService.Verify(i => i.Get(It.IsAny<Expression<Func<OrganizationLicense, bool>>>()));
            dataSchemaService.Verify(i => i.Get(dataSchema.ID));
        }
    }
}