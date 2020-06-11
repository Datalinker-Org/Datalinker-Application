using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using DataLinker.Services;
using DataLinker.Services.Configuration;
using DataLinker.Services.Emails;
using DataLinker.Services.Emails.Models;
using DataLinker.Services.Emails.Roles.Admin;
using DataLinker.Services.Emails.Roles.LegalOfficer;
using DataLinker.Services.Emails.Roles.User;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Organizations;
using DataLinker.Services.Organizations.Models;
using DataLinker.Services.Users;
using DataLinker.Services.Users.Models;
using DataLinker.Web.Controllers;
using DataLinker.Web.Models.Organizations;
using DataLinker.Web.Models.Users;
using Moq;
using NUnit.Framework;
using PagedList;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class OrganizationControllerTests
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

            user = new User
            {
                Email = "user@mail.email",
                ID = 2,
                IsSysAdmin = false,
                OrganizationID = 2
            };
            legalOfficer = new User
            {
                Email = "user@mail.email",
                ID = 3,
                IsSysAdmin = false,
                OrganizationID = 1,
                IsIntroducedAsLegalOfficer = true,
                IsVerifiedAsLegalOfficer = true
            };
            organization = new Organization
            {
                ID = 1,
                IsActive = true
            };

            var context = new Mock<ControllerContext>();
            context.Setup(m => m.HttpContext.Request.Url).Returns(new Uri("http://test.com"));

            userService = new Mock<IUserService>();
            organizationService = new Mock<IOrganizationService>();
            configurationService = new Mock<IConfigurationService>();
            var userNotification= new Mock<IUserNotificationService>();

            notificationService = new Mock<INotificationService>();
            notificationService.SetupGet(i => i.User).Returns(userNotification.Object);
            configurationService.SetupProperty(p => p.ManageOrganizationsPageSize, 5);
            userService.Setup(m => m.Get(user.ID)).Returns(user);
            userService.Setup(m => m.Get(It.IsAny<Expression<Func<User, bool>>>())).Returns(new List<User> {user});
            userService.Setup(m => m.GetLegalOfficers(organization.ID)).Returns(new List<User> {legalOfficer});
            organizationService.Setup(m => m.Get(organization.ID)).Returns(organization);
            organizationService.Setup(m => m.GetAllOrganizations()).Returns(new List<Organization> {organization});

            controller = new OrganizationController(userService.Object, organizationService.Object,
                notificationService.Object,configurationService.Object)
            {
                ControllerContext = context.Object
            };
        }

        private OrganizationController controller;
        private Organization organization;
        private Mock<IUserService> userService;
        private Mock<IOrganizationService> organizationService;
        private Mock<INotificationService> notificationService;
        private Mock<IConfigurationService> configurationService;
        private User user;
        private User sysAdmin;
        private User legalOfficer;

        [Test]
        public void AdminCanChangeStatusOfOrganiation()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            controller.ChangeStatus(organization.ID, false);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanChangeStatusOfOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.ChangeStatus(user.ID, true);
        }

        [Test]
        public void OnlyAdminSeeOrganizationList()
        {
            controller.LoggedInUser = new LoggedInUserDetails(sysAdmin);
            var result = (ViewResult) controller.Index(null);
            var model = (PagedList<OrganizationModel>) result.Model;
            organizationService.Verify(p => p.GetAllOrganizations());
            Assert.IsNotNull(model);
            Assert.IsTrue(model.Count == 1);
            Assert.IsTrue(model.First().ID == organization.ID);
        }
    }
}