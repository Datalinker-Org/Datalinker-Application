using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using DataLinker.Services.Applications;
using DataLinker.Services.Applications.Models;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseAgreements;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Services.Organizations;
using DataLinker.Services.Organizations.Models;
using DataLinker.Services.Schemas;
using DataLinker.Services.SoftwareStatements;
using DataLinker.Services.SoftwareStatements.Models;
using DataLinker.Services.Users;
using DataLinker.Services.Users.Models;
using DataLinker.Web.Controllers;
using DataLinker.Web.Models.Users;
using Moq;
using NUnit.Framework;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class SoftwareStatementControllerTests
    {
        private SoftwareStatementController controller;
        private Mock<ISoftwareStatementService> softwareStatementService;
        private Mock<IUserService> userService;
        private Mock<IOrganizationService> organizationService;
        private Mock<IApplicationsService> applicationService;
        private Mock<IApplicationTokenService> applicationTokenService;
        private Mock<IDataSchemaService> dataSchemaService;
        private Mock<IOrganizationLicenseService> organizationLicenseService;
        private Mock<ILicenseAgreementService> licenseAgreementService;
        private Application application;
        private Application otherApplication;
        private ApplicationToken appToken;
        private SoftwareStatement softwareStatement;
        private User userOrgMember;
        private User userNotOrgMember;
        private Organization organization;

        public void InitModels()
        {
            organization = new Organization
            {
                ID = 1,
                IsActive = true
            };
            application = new Application
            {
                ID = 1,
                IsProvider = false,
                OrganizationID = organization.ID
            };
            appToken = new ApplicationToken
            {
                ApplicationID = application.ID,
                ID = 1,
                Token = "token"
            };
            otherApplication = new Application
            {
                ID = 2,
                IsProvider = false,
                OrganizationID = organization.ID
            };
            userOrgMember = new User
            {
                ID=1,
                IsSysAdmin = false,
                OrganizationID = organization.ID
            };
            userNotOrgMember = new User
            {
                ID = 2,
                IsSysAdmin = false,
                OrganizationID = 2
            };
            softwareStatement = new SoftwareStatement
            {
                ApplicationID = application.ID,
                ID = 1,
                Content = "statement"
            };
        }

        [SetUp]
        public void Init()
        {
            InitModels();
            softwareStatementService = new Mock<ISoftwareStatementService>();
            userService = new Mock<IUserService>();
            organizationService = new Mock<IOrganizationService>();
            applicationTokenService = new Mock<IApplicationTokenService>();
            applicationService = new Mock<IApplicationsService>();
            dataSchemaService = new Mock<IDataSchemaService>();
            organizationLicenseService = new Mock<IOrganizationLicenseService>();
            licenseAgreementService = new Mock<ILicenseAgreementService>();
            // Setup software satement service
            softwareStatementService.Setup(
                i =>
                    i.GetNewStatement(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(),
                        It.IsAny<IApplicationsService>(), It.IsAny<IOrganizationLicenseService>(),
                        It.IsAny<IDataSchemaService>(), It.IsAny<ILicenseAgreementService>(),
                        It.IsAny<IApplicationTokenService>())).Returns(new SoftwareStatement {Content = string.Empty});
            // Setup application service
            applicationService.Setup(m => m.Get(application.ID)).Returns(application);
            applicationService.Setup(m => m.Get(otherApplication.ID)).Returns(otherApplication);
            // Setup application token service
            applicationTokenService.Setup(m => m.Get(It.IsAny<Expression<Func<ApplicationToken, bool>>>()))
                .Returns(new List<ApplicationToken> {appToken});
            applicationTokenService.Setup(m => m.GetFirstValidToken(application.ID)).Returns(appToken);
            // Setup software statement service
            softwareStatementService.Setup(m => m.GetValidStatement(application.ID)).Returns(softwareStatement);
            controller = new SoftwareStatementController(userService.Object, organizationService.Object,
                softwareStatementService.Object, applicationService.Object, applicationTokenService.Object,
                dataSchemaService.Object, organizationLicenseService.Object, licenseAgreementService.Object)
            {
                LoggedInUser = new LoggedInUserDetails(userOrgMember)
                {
                    Organization = new LoggedInOrganization(organization)
                }
            };
        }

        [Test]
        public void ShouldGetSoftwareStatement()
        {
            controller.Get(application.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetSoftwareStatementForIvalidApplication()
        {
            controller.Get(0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGetSoftwareStatementForNotMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization { ID = userNotOrgMember.OrganizationID.Value}
            };
            controller.Get(application.ID);
        }

        [Test]
        public void ShouldGenerateNewSoftwareStatement()
        {
            controller.GenerateNew(application.ID);
            softwareStatementService.Verify(m=>m.Update(It.IsAny<SoftwareStatement>()));
            softwareStatementService.Verify(m=>m.Add(It.IsAny<SoftwareStatement>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGenerateNewSoftwareStatementForInvalidApplication()
        {
            controller.GenerateNew(0);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailGenerateNewSoftwareStatementForNotMember()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userNotOrgMember)
            {
                Organization = new LoggedInOrganization { ID = userNotOrgMember.OrganizationID.Value }
            };
            controller.GenerateNew(application.ID);
        }

        [Test]
        public void ShouldFailGenerateNewSoftwareStatementIfNoStatementNotExist()
        {
            controller.GenerateNew(otherApplication.ID);
            softwareStatementService.Verify(m => m.Add(It.IsAny<SoftwareStatement>()));
        }
    }
}
