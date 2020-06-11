using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services;
using DataLinker.Services.Applications;
using DataLinker.Services.Applications.Models;
using DataLinker.Services.Configuration;
using DataLinker.Services.Emails;
using DataLinker.Services.Emails.Roles.Admin;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
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
using DataLinker.Services.Users;
using DataLinker.Services.Users.Models;
using DataLinker.Web.Controllers;
using DataLinker.Web.Models.Applications;
using DataLinker.Web.Models.ProviderEndpoints;
using DataLinker.Web.Models.Users;
using Moq;
using NUnit.Framework;
using PagedList;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class ApplicationsControllerTests
    {
        [SetUp]
        public void Init()
        {
            sysAdmin = new User
            {
                Email = "sysAdmin@mail.com",
                ID = 1,
                IsActive = true,
                IsSysAdmin = true,
                OrganizationID = 2
            };

            otherOrganization = new Organization
            {
                ID = 3,
                Name = "OrgName3",
                IsActive = true
            };

            user = new User
            {
                Email = "user@mail.email",
                ID = 2,
                IsSysAdmin = false,
                OrganizationID = 2
            };

            otherUser = new User
            {
                Email = "otherUser@mail.email",
                ID = 3,
                IsSysAdmin = false,
                OrganizationID = otherOrganization.ID
            };

            organization = new Organization
            {
                ID = 2,
                Name = "OrgName2",
                IsActive = true
            };


            activeService = new Application
            {
                OrganizationID = organization.ID,
                IsActive = true,
                Name = "activeService",
                PublicID = new Guid("421befd1-ef28-4c25-bcf6-5ead09dabb71"),
                ID = 1,
                IsIntroducedAsIndustryGood = true,
                IsProvider = true
            };

            consumerApplication = new Application
            {
                OrganizationID = 2,
                IsActive = true,
                Name = "active applications",
                PublicID = new Guid("421befd1-ef28-4c25-bcf6-5ead09dabb71"),
                ID = 4,
                IsProvider = false
            };

            notActiveService = new Application
            {
                OrganizationID = 2,
                IsActive = false,
                Name = "notActiveService",
                PublicID = new Guid("421befd1-ef28-4c25-bcf6-5ead09dabb71"),
                ID = 2,
                IsProvider = false
            };

            otherService = new Application
            {
                OrganizationID = 3,
                IsActive = true,
                Name = "otherService",
                PublicID = new Guid("421befd1-ef28-4c25-bcf6-5ead09dabb71"),
                ID = 3,
                IsProvider = false
            };

            dataSchema = new DataSchema
            {
                ID = 1,
                Name = "Schema1"
            };

            providerEndpoint = new ProviderEndpoint
            {
                ApplicationId = activeService.ID,
                ID = 1,
                DataSchemaID = dataSchema.ID,
                IsIndustryGood = true,
                Description = "Description"
            };

            licenseTemplate = new LicenseTemplate
            {
                ID = 1,
                LicenseID = 1,
                Status = (int)TemplateStatus.Active
            };

            _organizationLicense = new OrganizationLicense
            {
                ID = 1,
                Status = (int)PublishStatus.Published,
                ProviderEndpointID = providerEndpoint.ID,
                DataSchemaID = providerEndpoint.DataSchemaID,
                LicenseTemplateID = licenseTemplate.ID
            };
            applicationToken = new ApplicationToken
            {
                ID = 1,
                ApplicationID = activeService.ID,
                Token = "token"
            };

            appService = new Mock<IApplicationsService>();
            _userService = new Mock<IUserService>();
            orgService = new Mock<IOrganizationService>();
            schemaService = new Mock<IDataSchemaService>();
            endpointService = new Mock<IProviderEndpointService>();
            licenseTemplateService = new Mock<ILicenseTemplatesService>();
            sectionService = new Mock<ILicenseSectionService>();
            clauseService = new Mock<ILicenseClauseService>();
            clauseTemplateService = new Mock<ILicenseClauseTemplateService>();
            endpointLicClauseService = new Mock<IOrganizationLicenseClauseService>();
            licenseService = new Mock<IOrganizationLicenseService>();
            notificationService = new Mock<INotificationService>();
            applicationTokenService = new Mock<IService<ApplicationToken>>();
            applicationAuthenticationService = new Mock<IService<ApplicationAuthentication>>();
            configService = new Mock<IConfigurationService>();
            licenseContentBuilder = new Mock<ILicenseContentBuilder>();
            adminNotificationService = new Mock<IAdminNotificationService>();
            // Notification service
            notificationService.Setup(i => i.Admin).Returns(adminNotificationService.Object);
            configService.SetupProperty(p => p.ManageApplicationsPageSize, 5);
            var mockUrl = new Mock<UrlHelper>();

            // Setup application token service
            applicationTokenService.Setup(i => i.Get(applicationToken.ID)).Returns(applicationToken);
            applicationTokenService.Setup(i => i.Add(It.IsAny<ApplicationToken>())).Returns(true);
            applicationTokenService.Setup(i => i.Update(It.IsAny<ApplicationToken>())).Returns(true);
            // Schema service
            schemaService.Setup(p => p.Get(dataSchema.ID)).Returns(dataSchema);
            schemaService.Setup(p => p.Get(10)).Returns(new DataSchema {ID = 10, IsIndustryGood = false});
            schemaService.Setup(p => p.GetPublishedSchemas()).Returns(new List<DataSchema> { dataSchema, new DataSchema {ID = 10, IsIndustryGood = false} });

            // Endpoint service
            endpointService.Setup(p => p.Get(It.IsAny<Expression<Func<ProviderEndpoint, bool>>>()))
                .Returns(new List<ProviderEndpoint> { providerEndpoint});
            endpointService.Setup(p => p.Get(providerEndpoint.ID)).Returns(providerEndpoint);

            licenseService.Setup(p => p.Get(It.IsAny<Expression<Func<OrganizationLicense, bool>>>()))
                .Returns(new List<OrganizationLicense> {_organizationLicense});

            // License template service
            licenseTemplateService.Setup(p => p.GetAll(false)).Returns(new List<LicenseTemplate> {licenseTemplate});
            licenseTemplateService.Setup(p => p.GetPublishedGlobalLicense()).Returns(licenseTemplate);
            
            // Application service
            appService.Setup(p => p.GetApplicationsFor((int) user.OrganizationID))
                .Returns(new List<Application> {activeService, notActiveService});
            appService.Setup(p => p.GetAllApplications())
                .Returns(new List<Application> {activeService, otherService, notActiveService});
            appService.Setup(p => p.Get(activeService.ID)).Returns(activeService);
            appService.Setup(p => p.Get(notActiveService.ID)).Returns(notActiveService);
            appService.Setup(p => p.Get(otherService.ID)).Returns(otherService);
            appService.Setup(p => p.Get(consumerApplication.ID)).Returns(consumerApplication);
            appService.Setup(p => p.GetAuthenticationFor(activeService.ID)).Returns(new ApplicationAuthentication());

            // Organization service
            orgService.Setup(p => p.Get(organization.ID)).Returns(organization);
            orgService.Setup(p => p.Get(otherOrganization.ID)).Returns(otherOrganization);

            var context = new Mock<ControllerContext>();
            context.Setup(m => m.HttpContext.Request.Form).Returns(new FormCollection());
            context.Setup(m => m.HttpContext.Request.Url).Returns(new Uri("http://test.com"));
            context.Setup(m => m.HttpContext.Request.Browser).Returns(new Mock<HttpBrowserCapabilitiesBase>().Object);

            controller = new ApplicationsController(appService.Object, applicationTokenService.Object,
                applicationAuthenticationService.Object, _userService.Object,
                orgService.Object, schemaService.Object, endpointService.Object,
                licenseService.Object, configService.Object,
                notificationService.Object);
            controller.ControllerContext = context.Object;
            controller.Url = mockUrl.Object;
        }

        private ApplicationsController controller;

        private Mock<IApplicationsService> appService;
        private Mock<ILicenseContentBuilder> licenseContentBuilder;
        private Mock<IConfigurationService> configService;
        private Mock<IUserService> _userService;
        private Mock<IOrganizationService> orgService;
        private Mock<IProviderEndpointService> endpointService;
        private Mock<IDataSchemaService> schemaService;
        private Mock<ILicenseTemplatesService> licenseTemplateService;
        private Mock<ILicenseSectionService> sectionService;
        private Mock<ILicenseClauseService> clauseService;
        private Mock<ILicenseClauseTemplateService> clauseTemplateService;
        private Mock<IOrganizationLicenseClauseService> endpointLicClauseService;
        private Mock<IOrganizationLicenseService> licenseService;
        private Mock<INotificationService> notificationService;
        private Mock<IAdminNotificationService> adminNotificationService;
        private Mock<IService<ApplicationToken>> applicationTokenService;
        private Mock<IService<ApplicationAuthentication>> applicationAuthenticationService;
        private Application notActiveService;
        private Application otherService;
        private Application activeService;
        private Application consumerApplication;
        private ApplicationToken applicationToken;
        private Organization otherOrganization;
        private ProviderEndpoint providerEndpoint;
        private OrganizationLicense _organizationLicense;
        private DataSchema dataSchema;
        private LicenseTemplate licenseTemplate;

        private User sysAdmin;
        private User user;
        private User otherUser;
        private Organization organization;

        [Test]
        public void AdminCanChangeStatusOfService()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.ChangeStatus(activeService.ID, true);
            appService.Verify(m => m.Get(activeService.ID));
            appService.Verify(m => m.Update(activeService));
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanChangeStatusOfService()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.ChangeStatus(activeService.ID, true);
        }

        [Test]
        public void ShouldReturnDetailsOfServiceForAdmin()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            var result = (ViewResult) controller.Details(otherService.ID);
            var model = (ApplicationDetails) result.Model;
            Assert.NotNull(model);
            Assert.IsTrue(model.ID == otherService.ID);
            Assert.IsTrue(model.OrganizationID != user.OrganizationID);
        }

        [Test]
        public void ShouldReturnDetailsOfServiceForUser()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (ViewResult) controller.Details(activeService.ID);
            var model = (ApplicationDetails) result.Model;
            Assert.NotNull(model);
            Assert.IsTrue(model.ID == activeService.ID);
            Assert.IsTrue(model.OrganizationID == user.OrganizationID);
        }

        [Test]
        public void ShouldReturnListOfServicesForUser()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (ViewResult) controller.Index(null);
            var model = (IEnumerable<ApplicationDetails>) result.Model;
            Assert.NotNull(model);
            Assert.IsTrue(model.Count() == 2);
            Assert.IsTrue(model.First().IsActive);
        }

        [Test]
        public void ShouldReturnListOfServicesOfAllOrganizationsForAdmin()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            var result = (ViewResult) controller.Index(null);
            var model = (IPagedList<ApplicationDetails>) result.Model;
            Assert.NotNull(model);
            Assert.IsTrue(model.Count() == 3);
            Assert.IsTrue(model.Any(p => p.IsActive));
            Assert.IsTrue(model.Any(p => p.OrganizationID != user.OrganizationID));
            Assert.NotNull(model.Select(p => p.ID == activeService.ID));
            Assert.NotNull(model.Select(p => p.ID == otherService.ID));
        }

        [Test]
        public void UserCanFilterServicesByStatus()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (ViewResult) controller.Index(null);
            var model = (IPagedList<ApplicationDetails>) result.Model;
            Assert.NotNull(model);
            Assert.IsTrue(model.Count() == 2);
            Assert.IsFalse(model.Any(p => p.OrganizationID != user.OrganizationID));
            Assert.IsNotNull(model.Select(p => p.ID == notActiveService.ID));
        }

        [Test]
        public void UserCanViewListOfServicesForHisOrganizationOnly()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (ViewResult) controller.Index(null);
            var model = (IPagedList<ApplicationDetails>) result.Model;
            Assert.NotNull(model);
            Assert.IsTrue(model.Count() == 2);
            Assert.IsFalse(model.Any(p => p.OrganizationID != user.OrganizationID));
            Assert.IsNotNull(model.FirstOrDefault(p => p.ID == activeService.ID));
        }
        
        [Test]
        public void ShouldAddProviderEndpoint()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult)controller.AddProviderEndpoint(activeService.ID);
            var model = (ProviderEndpointModel)result.Model;
            model.SelectedSchemaID = 10;
            model.Name = "new name";
            Assert.IsNotNull(model.Schemas);
            controller.AddProviderEndpoint(activeService.ID, model);
            endpointService.Verify(m=>m.Add(It.IsAny<ProviderEndpoint>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void NotMemberOfOrganizationShouldNotAddProviderEndpoint()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.AddProviderEndpoint(activeService.ID, new ProviderEndpointModel());
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailAddProviderEndpointForNotActiveOrganization()
        {
            otherOrganization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.AddProviderEndpoint(activeService.ID, new ProviderEndpointModel());
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailAddProviderEndpointIfEndpointAlreadyExists()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.AddProviderEndpoint(activeService.ID, new ProviderEndpointModel {Name = providerEndpoint.Name});
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void NotMemberShouldNotSeeAddEndpointPage()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.AddProviderEndpoint(activeService.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailDisplayAddEndpointForNotActiveOrganization()
        {
            otherOrganization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.AddProviderEndpoint(activeService.ID);
        }

        [Test]
        public void AdminShouldApproveIndustryGoodApp()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.ApproveIndustryGood(activeService.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void UserShouldNotApproveIndustryGoodApp()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.ApproveIndustryGood(activeService.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotApproveApprovedIndustryGoodApp()
        {
            activeService.IsIntroducedAsIndustryGood = true;
            activeService.IsVerifiedAsIndustryGood = true;
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.ApproveIndustryGood(activeService.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotApproveNotIndustryGoodApp()
        {
            activeService.IsIntroducedAsIndustryGood = false;
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.ApproveIndustryGood(activeService.ID);
        }

        [Test]
        public void AdminShouldDeclineIndustryGoodApp()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.DeclineIndustryGood(activeService.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void UserShouldNotDeclineIndustryGoodApp()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.DeclineIndustryGood(activeService.ID);
            }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotDeclineApprovedIndustryGoodApp()
        {
            activeService.IsIntroducedAsIndustryGood = true;
            activeService.IsVerifiedAsIndustryGood = true;
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.DeclineIndustryGood(activeService.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotDeclineNotIndustryGoodApp()
        {
            activeService.IsIntroducedAsIndustryGood = false;
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.DeclineIndustryGood(activeService.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateApplicationIfOrganizationNotActive()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Create(new NewApplicationDetails());
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateApplicationIfApplicationNameInUse()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Create(new NewApplicationDetails {Name = activeService.Name});
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateApplicationForAdmin()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.Create(new NewApplicationDetails { Name = activeService.Name });
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateApplicationIfHostsNotSpecified()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Create(new NewApplicationDetails { Name = "applicationname" });
        }

        [Test]
        public void ShouldCreateApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Create(new NewApplicationDetails { Name = "applicationname", OriginHosts = "http://test.co.nz"});
            appService.Verify(i=>i.Add(It.IsAny<Application>(), It.IsAny<ApplicationAuthentication>(), It.IsAny<List<ApplicationToken>>()));
        }

        [Test]
        public void ShouldCreateApplicationAndNotifyAdminAboutIndustryGood()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Create(new NewApplicationDetails { Name = "applicationname", OriginHosts = "http://test.co.nz",IsIntroducedAsIndustryGood = true});
            appService.Verify(i => i.Add(It.IsAny<Application>(), It.IsAny<ApplicationAuthentication>(), It.IsAny<List<ApplicationToken>>()));
            adminNotificationService.Verify(
                m => m.NewIndustryGoodApplicationInBackground(It.IsAny<string>(), It.IsAny<int>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateApplicationForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Create("service");
        }

        [Test]
        public void ShouldReturnCreateConsumerApplicationView()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult)controller.Create("application");
            var model = (NewApplicationDetails)result.Model;
            Assert.IsFalse(model.IsProvider);
            Assert.IsTrue(result.ViewBag.AppType.ToString() == "consumer");
            Assert.IsTrue(result.ViewBag.AppTypeDescription.ToString() == "application");
        }

        [Test]
        public void ShouldReturnCreateProviderServiceView()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult)controller.Create("service");
            var model = (NewApplicationDetails)result.Model;
            Assert.IsTrue(model.IsProvider);
            Assert.IsTrue(result.ViewBag.AppType.ToString() == "provider");
            Assert.IsTrue(result.ViewBag.AppTypeDescription.ToString() == "service");
        }

        [Test]
        public void ShouldReturnFalseForApplicationNameThatAlreadyInUse()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = controller.IsApplicationNotExistsForThisOrganization(activeService.Name, String.Empty);
            Assert.IsFalse((bool)result.Data);
        }

        [Test]
        public void ShouldReturnTrueForApplicationNameThatAlreadyInUse()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = controller.IsApplicationNotExistsForThisOrganization("newName", String.Empty);
            Assert.IsTrue((bool)result.Data);
        }

        [Test]
        public void ShouldReturnFalseForEndpointNameThatAlreadyInUse()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = controller.IsEdnpointNotExistsForThisOrganization(providerEndpoint.Name, String.Empty);
            Assert.IsFalse((bool)result.Data);
        }

        [Test]
        public void ShouldReturnTrueForEndpointNameThatAlreadyInUse()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = controller.IsEdnpointNotExistsForThisOrganization("newName", String.Empty);
            Assert.IsTrue((bool)result.Data);
        }

        [Test]
        public void ShouldAllowAdminViewEditApplicationPage()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            var result = (PartialViewResult)controller.Edit(consumerApplication.ID);
            var model = (ApplicationDetails)result.Model;
            Assert.IsFalse(model.IsProvider);
            Assert.IsTrue(result.ViewBag.AppType.ToString() == "application");
        }

        [Test]
        public void ShouldAllowAdminViewEditServicePage()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            var result = (PartialViewResult)controller.Edit(activeService.ID);
            var model = (ApplicationDetails)result.Model;
            Assert.IsTrue(model.IsProvider);
            Assert.IsTrue(result.ViewBag.AppType.ToString() == "service");
        }

        [Test]
        public void ShouldReturnNotFoundForNotExistingApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (HttpStatusCodeResult)controller.Edit(0);
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldNotAllowViewEditApplicationPageForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.Edit(activeService.ID);
        }

        [Test]
        public void ShouldReturnEditServiceView()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult)controller.Edit(consumerApplication.ID);
            var model = (ApplicationDetails)result.Model;
            Assert.IsFalse(model.IsProvider);
            Assert.IsTrue(result.ViewBag.AppType.ToString() == "application");
        }

        [Test]
        public void ShouldReturnEditApplicationView()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult)controller.Edit(activeService.ID);
            var model = (ApplicationDetails)result.Model;
            Assert.IsTrue(model.IsProvider);
            Assert.IsTrue(result.ViewBag.AppType.ToString() == "service");
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailEditApplicationIfOrganizationIsNotActive()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationDetails(activeService);
            controller.Edit(activeService.ID, model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailEditApplicationForNotOrganizationMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            var model = new ApplicationDetails(activeService);
            controller.Edit(activeService.ID, model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailEditApplicationIfApplicationNameAlreadyInUse()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationDetails(activeService);
            model.Name = notActiveService.Name;
            controller.Edit(activeService.ID, model);
        }

        [Test]
        public void ShouldReturnNotFoundWhenEditNotExistingApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationDetails(new Application());
            var result = (HttpStatusCodeResult)controller.Edit(0, model);
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        public void ShouldEditIfIsIntroducedAsIndustryGoodWasChanged()
        {
            activeService.IsIntroducedAsIndustryGood = true;
            activeService.IsVerifiedAsIndustryGood = true;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationDetails(activeService);
            model.IsIntroducedAsIndustryGood = false;
            controller.Edit(activeService.ID, model);
            Assert.IsFalse(activeService.IsVerifiedAsIndustryGood);
            appService.Verify(i => i.Update(It.IsAny<Application>()));
        }

        [Test]
        public void ShouldMakeApplicationNotActiveIfApplicationMarkedAsIndustryGood()
        {
            activeService.IsVerifiedAsIndustryGood = false;
            activeService.IsIntroducedAsIndustryGood = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationDetails(activeService);
            model.IsIntroducedAsIndustryGood = true;
            controller.Edit(activeService.ID, model);
            Assert.IsFalse(activeService.IsActive);
            adminNotificationService.Verify(i => i.NewIndustryGoodApplicationInBackground(It.IsAny<string>(), organization.ID));
            appService.Verify(i => i.Update(It.IsAny<Application>()));
        }

        [Test]
        public void ShouldAllowEditApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationDetails(activeService);
            controller.Edit(activeService.ID, model);
            appService.Verify(i => i.Update(It.IsAny<Application>()));
        }

        [Test]
        public void ShouldAllowAdminEditApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationDetails(activeService);
            controller.Edit(activeService.ID, model);
            appService.Verify(i=>i.Update(It.IsAny<Application>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetApplicationHostsIfApplicationNotFound()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.GetApplicationHosts(0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetApplicationHostsIfUserNotRelatesToApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            controller.GetApplicationHosts(activeService.ID);
            appService.Verify(i => i.Get(activeService.ID));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetApplicationHostsIfUserWithoutOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.GetApplicationHosts(activeService.ID);
            appService.Verify(i => i.Get(activeService.ID));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetApplicationHostsForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.GetApplicationHosts(activeService.ID);
            appService.Verify(i => i.Get(activeService.ID));
        }

        [Test]
        public void ShouldGetApplicationHosts()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult)controller.GetApplicationHosts(activeService.ID);
            appService.Verify(i=>i.Get(activeService.ID));
            Assert.IsNotNull(result.Model);
        }

        [Test]
        public void ShouldGetApplicationHostsForAdmin()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult)controller.GetApplicationHosts(activeService.ID);
            appService.Verify(i => i.Get(activeService.ID));
            Assert.IsNotNull(result.Model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailReturnAddNewHostViewForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.AddNewHost(activeService.ID);
        }

        [Test]
        public void ShouldReturnNotFoundForAddNewHostViewIfApplicationNotExists()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            
            var result = (HttpStatusCodeResult)controller.AddNewHost(0);
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        public void ShouldReturnAddNewHostView()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult) controller.AddNewHost(activeService.ID);
            Assert.IsNotNull(result.Model);
        }

        [Test]
        public void ShouldFailToAddNewHostIfOrganizationIsNotActive()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new FormCollection();
            model.Add("Host", "http://hostname.co.nz");
            controller.AddNewHost(activeService.ID, model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailToAddNewHostForNotOrganizationMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            var model = new FormCollection();
            model.Add("newHostName", "http://hostname.co.nz");
            controller.AddNewHost(activeService.ID, model);
        }

        [Test]
        public void ShouldFailToAddNewHostForNotExistingApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new FormCollection {{"newHostName", "http://hostname.co.nz"}};
            var result = (HttpStatusCodeResult)controller.AddNewHost(0, model);
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        public void ShouldAddNewHost()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new FormCollection();
            model.Add("Host","http://hostname.co.nz");
            controller.AddNewHost(activeService.ID, model);
            applicationTokenService.Verify(i=>i.Add(It.IsAny<ApplicationToken>()));
        }

        [Test]
        public void ShouldGetApplicationAuthentication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (PartialViewResult) controller.GetApplicationAuthentication(activeService.ID);
            Assert.IsNotNull(result.Model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetApplicationAuthenticationIfOrganizationIsNotActive()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.GetApplicationAuthentication(activeService.ID);
        }

        [Test]
        public void ShouldFailGetApplicationAuthenticationIfApplicationNotExists()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var result = (HttpStatusCodeResult)controller.GetApplicationAuthentication(0);
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailReturnEditApplicationAuthenticationPageForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.EditApplicationAuthentication(activeService.ID);
        }

        [Test]
        public void ShouldReturnEditApplicationAuthenticationPageForNotActiveOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.EditApplicationAuthentication(activeService.ID);
        }

        [Test]
        public void ShouldFailReturnEditApplicationAuthenticationPageForNotExistingApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            
            var result = (HttpStatusCodeResult)controller.EditApplicationAuthentication(0);
            appService.Verify(i => i.Get(0));
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        public void ShouldEditApplicationAuthentication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationAuthenticationDetails();
            controller.EditApplicationAuthentication(activeService.ID, model);
            appService.Verify(i => i.Get(activeService.ID));
            applicationAuthenticationService.Verify(i => i.Update(It.IsAny<ApplicationAuthentication>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailEditApplicationAuthenticationForNotOrganizationMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };

            var model = new ApplicationAuthenticationDetails();
            controller.EditApplicationAuthentication(activeService.ID, model);
        }

        [Test]
        public void ShouldFailEditApplicationAuthenticationForNotExistingApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            var model = new ApplicationAuthenticationDetails();
            var result = (HttpStatusCodeResult)controller.EditApplicationAuthentication(0, model);
            appService.Verify(i => i.Get(0));
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGenerateNewTokenForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.GenerateNewToken(0, 0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGenerateNewTokenForAnotherUser()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };
            controller.GenerateNewToken(activeService.ID, 0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGenerateNewTokenForInvalidApplication()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.GenerateNewToken(0, 0);
            appService.Verify(m => m.Get(0));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGenerateNewTokenForInvalidAppToken()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.GenerateNewToken(activeService.ID, 0);
            appService.Verify(m=>m.Get(activeService.ID));
            applicationTokenService.Verify(m => m.Get(0));
        }

        [Test]
        public void ShouldGenerateNewToken()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.GenerateNewToken(activeService.ID, applicationToken.ID);
            appService.Verify(m => m.Get(activeService.ID));
            applicationTokenService.Verify(m=>m.Get(applicationToken.ID));
            applicationTokenService.Verify(m=>m.Add(It.IsAny<ApplicationToken>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailExpireTokenForAnotherUser()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(otherOrganization)
            };
            controller.ExpireToken(activeService.ID, 0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailExpireTokenForNotActiveOrganization()
        {
            organization.IsActive = false;
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.ExpireToken(0, 0);
            appService.Verify(m => m.Get(0));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailExpireTokenForInvalidAppToken()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.ExpireToken(activeService.ID, 0);
            appService.Verify(m => m.Get(activeService.ID));
            applicationTokenService.Verify(m => m.Get(0));
        }

        [Test]
        public void ShouldExpireToken()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.ExpireToken(activeService.ID,applicationToken.ID);
            appService.Verify(m => m.Get(activeService.ID));
            applicationTokenService.Verify(m => m.Get(applicationToken.ID));
            applicationTokenService.Verify(m => m.Update(It.IsAny<ApplicationToken>()));
        }
    }
}