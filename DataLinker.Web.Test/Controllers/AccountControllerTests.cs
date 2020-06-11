using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services;
using DataLinker.Services.Configuration;
using DataLinker.Services.Emails;
using DataLinker.Services.Emails.Roles.Admin;
using DataLinker.Services.Emails.Roles.User;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Organizations;
using DataLinker.Services.Organizations.Models;
using DataLinker.Services.Users;
using DataLinker.Services.Users.Models;
using DataLinker.Web.Controllers;
using DataLinker.Web.Helpers;
using DataLinker.Web.Models;
using DataLinker.Web.Models.Users;
using Microsoft.Owin.Security;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<IUserService> userService { get; set; }
        private Mock<IOrganizationService> organizationService { get; set; }
        private Mock<IConfigurationService> configurationService { get; set; }
        private Mock<INotificationService> notificationService { get; set; }
        private Mock<IUserNotificationService> userNotificationService { get; set; }
        private Mock<IAdminNotificationService> adminNotificationService { get; set; }
        public AccountController controller { get; set; }
        public Mock<IIdentityServerRequestHelper> identityServer { get; set; }
        public Mock<IAuthenticationManager> authenticationManager { get; set; }
        public Mock<UrlHelper> mockUrl { get; set; }
        public Mock<ControllerContext> context { get; set; }
        public Organization organization { get; set; }

        public User userSysAdmin { get; set; }
        public User user { get; set; }
        public User otherUser { get; set; }

        [SetUp]
        public void Init()
        {
            this.userSysAdmin = new User
            {
                Email = "sysAdmin@mail.com",
                ID = 1,
                IsActive = true,
                IsSysAdmin = true,
                OrganizationID = 2
            };

            this.organization = new Organization
            {
                ID = 2,
                Name = "OrgName"
            };

            this.user = new User
            {
                ID = 2,
                Email = "user1@mail.com",
                IsActive = false,
                OrganizationID = 2,
                IsSysAdmin = false,
                IsIntroducedAsLegalOfficer = true
            };

            this.otherUser = new User
            {
                ID = 3,
                Email = "user2@mail.com",
                IsActive = true,
                OrganizationID = 2,
                IsSysAdmin = false,
                IsIntroducedAsLegalOfficer = true
            };

            userService = new Mock<IUserService>();
            organizationService = new Mock<IOrganizationService>();
            configurationService = new Mock<IConfigurationService>();
            notificationService = new Mock<INotificationService>();
            authenticationManager = new Mock<IAuthenticationManager>();
            identityServer = new Mock<IIdentityServerRequestHelper>();
            userNotificationService= new Mock<IUserNotificationService>();
            adminNotificationService = new Mock<IAdminNotificationService>();

            notificationService.SetupGet(i => i.User).Returns(userNotificationService.Object);
            notificationService.SetupGet(i => i.Admin).Returns(adminNotificationService.Object);

            this.userService.Setup(m => m.Get(true, 2)).Returns(new List<User> {userSysAdmin});
            this.userService.Setup(m => m.Get(true, organization.ID)).Returns(new List<User> {userSysAdmin});
            this.userService.Setup(m => m.Get(false, 2)).Returns(new List<User> {userSysAdmin, user});
            this.userService.Setup(m => m.Get(user.ID)).Returns(user);
            this.userService.Setup(m => m.Get(otherUser.ID)).Returns(otherUser);
            this.userService.Setup(m => m.GetAll()).Returns(new List<User> { user, otherUser, userSysAdmin});
            this.userService.Setup(m => m.GetLegalOfficers(organization.ID)).Returns(new List<User> { user});
            this.organizationService.Setup(m => m.Get((int) userSysAdmin.OrganizationID)).Returns(this.organization);
            this.configurationService.SetupGet(m => m.ConfirmNewEmailExpires).Returns("24");
            this.configurationService.SetupGet(m => m.ManageUsersPageSize).Returns(5);
            var idUser = new Rezare.IdentityServer.Common.Models.User();
            identityServer.Setup(
                i =>
                    i.CreateUser(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                        It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(idUser);
            context = new Mock<ControllerContext>();
            context.Setup(m => m.HttpContext.Request.UrlReferrer).Returns(new Uri("http://test.com"));
            context.Setup(m => m.HttpContext.Request.Browser).Returns(new Mock<HttpBrowserCapabilitiesBase>().Object);
            context.Setup(m => m.HttpContext.Request.Url).Returns(new Uri("http://test.com"));
            context.Setup(m => m.HttpContext.User.Identity.IsAuthenticated).Returns(true);

            mockUrl = new Mock<UrlHelper>();
            this.controller = new AccountController(userService.Object, organizationService.Object,
                configurationService.Object, notificationService.Object, authenticationManager.Object, identityServer.Object)
            {
                ControllerContext = context.Object,
                Url = mockUrl.Object,
                LoggedInUser = new LoggedInUserDetails(userSysAdmin)
            };
        }
        
        [Test]
        public void ShouldReturnListOfUsers()
        {

            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (ViewResult) controller.Index(null,organizationID:organization.ID);

            userService.Verify(m => m.Get(true, organization.ID));
            organizationService.Verify(m => m.Get(It.IsAny<int>()));
            var model = (PagedList.IPagedList<UserModel>) result.Model;
            Assert.IsTrue(model.Count() == 1);
            Assert.IsTrue(model.First().ID == userSysAdmin.ID);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminShouldSeeListOfUsers()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            var result = (ViewResult) controller.Index(null);
        }

        [Test]
        public void ShouldReturnInActiveAndActiveUsers()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (ViewResult) controller.Index(null, false,organization.ID);

            userService.Verify(m => m.Get(false, 2));
            organizationService.Verify(m => m.Get(It.IsAny<int>()));
            var model = (PagedList.IPagedList<UserModel>) result.Model;
            Assert.IsTrue(model.Count() == 2);
            Assert.IsTrue(model.Any(p => p.IsActive == false));
        }

        [Test]
        public void ShouldReturnUsersWithTheSameOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (ViewResult) controller.Index(null, organizationID: organization.ID);

            userService.Verify(m => m.Get(true, organization.ID));
            organizationService.Verify(m => m.Get(It.IsAny<int>()));
            var model = (PagedList.IPagedList<UserModel>) result.Model;
            Assert.IsTrue(model.Count() == 1);
            Assert.IsTrue(model.First().OrganizationName == organization.Name);
        }

        [Test]
        public void UserShouldEditOwnUserProfile()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (PartialViewResult) controller.Edit(user.ID);

            userService.Verify(m => m.Get(user.ID));
            organizationService.Verify(m => m.Get((int) user.OrganizationID));
            var model = (UserModel) result.Model;
            Assert.IsTrue(model.ID == user.ID);
        }

        [Test]
        public void AdminShouldEditUserProfile()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            var result = (PartialViewResult) controller.Edit(user.ID);

            userService.Verify(m => m.Get(user.ID));
            organizationService.Verify(m => m.Get((int) user.OrganizationID));
            var model = (UserModel) result.Model;
            Assert.IsTrue(model.ID == user.ID);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void UserCanEditOnlyOwnProfile()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(organization)
            };
            controller.Edit(user.ID);
            userService.Verify(m => m.Get(user.ID));
        }

        [Test]
        public async Task AdminCanChangeStatusOfUserWhenEdit()
        {
            // Initialization
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            var result = (PartialViewResult) controller.Edit(user.ID);
            userService.Verify(m => m.Get(user.ID));
            organizationService.Verify(m => m.Get((int) user.OrganizationID));
            var model = (UserModel) result.Model;
            Assert.IsTrue(model.ID == user.ID);
            model.IsActive = true;
            Assert.IsFalse(user.IsActive);

            // Execution
            await controller.Edit(model.ID, model);

            // Assertion
            userService.Verify(m => m.Update(user));
            Assert.IsTrue(user.IsActive);
        }

        [Test]
        public async Task UserCanNotChangeHisStatus()
        {
            // Initialization
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            var result = (PartialViewResult) controller.Edit(user.ID);
            userService.Verify(m => m.Get(user.ID));
            organizationService.Verify(m => m.Get((int) user.OrganizationID));
            var model = (UserModel) result.Model;
            Assert.IsTrue(model.ID == user.ID);
            model.IsActive = true;
            Assert.IsFalse(user.IsActive);

            // Execution
            await controller.Edit(model.ID,model);

            // Assertion
            userService.Verify(m => m.Update(user));
            Assert.IsFalse(user.IsActive);
        }

        [Test]
        public void AdminCanChangeStatusOfUser()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            controller.ChangeStatus(user.ID, true);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void NotOrgMemberCanNotChangeStatusOfUser()
        {
            controller.LoggedInUser = new LoggedInUserDetails(otherUser)
            {
                Organization = new LoggedInOrganization(new Organization {ID = 9})
            };

            controller.ChangeStatus(user.ID, true);
        }

        [Test]
        public void AdminCanSeeDetailsOfUserAccount()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            var result = (ViewResult) controller.Details(user.ID);
            var model = (UserModel) result.Model;
            Assert.IsTrue(model.ID == user.ID);
            Assert.IsTrue(model.IsActive == user.IsActive);
            Assert.IsTrue(model.Email == user.Email);

        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void UserCanNotSeeDetailsOfUserAccount()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Details(user.ID);

        }

        [Test]
        public async Task UserCanChangeEmailViaEmailVerificationProcess()
        {
            // Inititalization
            var result = (PartialViewResult) controller.Edit(user.ID);
            userService.Verify(m => m.Get(user.ID));
            organizationService.Verify(m => m.Get((int) user.OrganizationID));
            var model = (UserModel) result.Model;
            model.Email = "newEmail@mail.com";

            // Execution
            await controller.Edit(model.ID, model);

            //Assertion
            Assert.IsTrue(user.NewEmail == model.Email);
            userService.Verify(m => m.Update(It.IsAny<User>()));
            Assert.IsNotNull(user.Token);
            Assert.IsNotNull(user.TokenExpire);
            Assert.IsNotNull(user.NewEmail);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public async Task UserCanNotUseInvalidEmailFormatForEmail()
        {
            // Inititalization
            var result = (PartialViewResult) controller.Edit(user.ID);
            userService.Verify(m => m.Get(user.ID));
            organizationService.Verify(m => m.Get((int) user.OrganizationID));
            var model = (UserModel) result.Model;
            model.Email = "badEmail.com";

            // Execution
            await controller.Edit(model.ID, model);
        }

        [Test]
        public async Task ShouldFailConfirmEmailIfTokenExpired()
        {
            // Initialization
            user.Token = "917a6cb7-b099-4148-a6a6-92ffb2fd5be7";
            user.TokenExpire = DateTime.Parse("2016-01-21 08:43:52.0000000");
            var newEmail = "newEmail@mail.com";
            var token = "AgAAAABMmTCbItOIt2x6kZmwSEGmppL/sv1b5w==";
            user.NewEmail = newEmail;
            user.ID = 2;
            controller.LoggedInUser = new LoggedInUserDetails(user);

            // Execution
            var result = await this.controller.ConfirmEmail(token);
            var viewResult = (ViewResult) result;

            // Assertion
            Assert.AreEqual(viewResult.ViewName, "EmailLinkExpired");
        }

        [Test]
        public void ShouldFailConfirmUserIfTokenExpired()
        {
            // Initialization
            user.Token = "917a6cb7-b099-4148-a6a6-92ffb2fd5be7";
            user.TokenExpire = DateTime.Parse("2016-01-21 08:43:52.0000000");
            var newEmail = "newEmail@mail.com";
            var token = "AgAAAABMmTCbItOIt2x6kZmwSEGmppL/sv1b5w==";
            user.NewEmail = newEmail;
            user.ID = 2;
            controller.LoggedInUser = new LoggedInUserDetails(user);

            // Execution
            var result = this.controller.ConfirmUser(token);
            var viewResult = (ViewResult)result;

            // Assertion
            Assert.AreEqual(viewResult.ViewName, "EmailLinkExpired");
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailConfirmUserIfTokenNotSpecified()
        {
            this.controller.ConfirmUser(string.Empty);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public async Task ShouldFailConfirmEmailIfTokenNotSpecified()
        {
            await this.controller.ConfirmEmail(string.Empty);
        }

        [Test]
        public void UserShouldCreateNewUserAndOrganization()
        {
            var model = new UserOrganizationModel
            {
                AdministrativeContact = "contact",
                Email = "newEmail@mail.com",
                AdministrativePhone = "phone",
                IsAgreeWithTerms = true,
                OrganizationAddress = "address",
                OrganizationName = "newOrg",
                OrganizationPhone = "phone",
                Phone = "phone",
                TermsOfService = "Terms",
                Name = "name"
            };

            // Execution
            this.controller.Create(model);

            // Assertion
            this.organizationService.Verify(m => m.Add(It.IsAny<Organization>()));
            this.userService.Verify(m => m.Add(It.IsAny<User>()));
            this.userService.Verify(m => m.Update(It.IsAny<User>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateUserAndOrganizationWhenEmailNotValid()
        {
            var model = new UserOrganizationModel
            {
                Email = "BadEmailmail.com"
            };

            // Execution
            this.controller.Create(model);
        }
        

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateUserAndOrganizationWhenEmailInUse()
        {
            var model = new UserOrganizationModel
            {
                Email = user.Email
            };

            // Execution
            this.controller.Create(model);
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateUserAndOrganizationWhenOrganizationNameInUse()
        {
            var model = new UserOrganizationModel
            {
                Email = "newEmail@mail.com",
                OrganizationName = organization.Name,
            };

            // Execution
            this.controller.Create(model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateUserAndOrganizationIfOrganizationNameNotSpecified()
        {
            var model = new UserOrganizationModel
            {
                Email = "newEmail@mail.com"
            };

            // Execution
            this.controller.Create(model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateUserAndOrganizationIfOrganizationContactNotSpecified()
        {
            var model = new UserOrganizationModel
            {
                Email = "newEmail@mail.com",
                OrganizationName = "name"
            };

            // Execution
            this.controller.Create(model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateUserAndOrganizationIfOrganizationPhoneNotSpecified()
        {
            var model = new UserOrganizationModel
            {
                Email = "newEmail@mail.com",
                OrganizationName = "name",
                AdministrativeContact = "contact"
            };

            // Execution
            this.controller.Create(model);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailCreateUserAndOrganizationIfUserNotAgreeWithTerms()
        {
            var model = new UserOrganizationModel
            {
                Email = "newEmail@mail.com",
                OrganizationName = "name",
                AdministrativeContact = "contact",
                AdministrativePhone = "123"
            };

            // Execution
            this.controller.Create(model);
        }

        [Test]
        public void AdminShouldApproveLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            controller.ApproveLegalOfficer(user.ID);
            userService.Verify(m=>m.Get(user.ID));
            userService.Verify(m=>m.Update(user));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void AdminShouldFailApproveLegalOfficerIfUserAlreadyLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            user.IsIntroducedAsLegalOfficer = true;
            user.IsVerifiedAsLegalOfficer = true;
            controller.ApproveLegalOfficer(user.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void AdminShouldFailDeclineLegalOfficerIfUserAlreadyLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            user.IsIntroducedAsLegalOfficer = true;
            user.IsVerifiedAsLegalOfficer = true;
            controller.DeclineLegalOfficer(user.ID);
        }

        [Test]
        public void AdminShouldDeclineLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            controller.DeclineLegalOfficer(user.ID);
            userService.Verify(m => m.Get(user.ID));
            userService.Verify(m => m.Update(user));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanApproveUserAsLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.ApproveLegalOfficer(user.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanDeclineUserAsLegalOfficer()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.DeclineLegalOfficer(user.ID);
        }

        [Test]
        public void ShouldAddNewMemberToOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            
            controller.AddNewMember(new UserModel(new User { Email = "newEmail@mail.com", OrganizationID = organization.ID }));
            userService.Verify(i=>i.Add(It.IsAny<User>()));
            userService.Verify(i=>i.Update(It.IsAny<User>()));
        }

        [Test]
        public void ShouldAllowAdminAddNewMemberToOrganization()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            controller.AddNewMember(new UserModel(new User {Email = "newEmail@mail.com", OrganizationID = organization.ID}));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailNotOrgMemberAddNewMember()
        {
            organization.ID++;
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            controller.AddNewMember(new UserModel(user));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailAddNewMemberIfOrganizationNotFound()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            user.OrganizationID = organization.ID + 1;
            controller.AddNewMember(new UserModel(user));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailAddNewOrgMemberIfEmailIsNotValid()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };

            user.Email = "emailInBadFormat";
            controller.AddNewMember(new UserModel(user));
        }

        [Test]
        public void ShouldReturnFalseForUserEmail()
        {
            Assert.IsFalse((bool)controller.IsEmailNotExists(user.Email,"").Data);
        }

        [Test]
        public void ShouldReturnTrueForUserEmail()
        {
            Assert.IsTrue((bool)controller.IsEmailNotExists("newEmail@mail.com",user.Email).Data);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailAddNewMemberWithExistingEmail()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user)
            {
                Organization = new LoggedInOrganization(organization)
            };
            
            controller.AddNewMember(new UserModel(user));
        }

        [Test]
        public void ShouldCheckAuthorizationForUser()
        {
            var result = controller.IsAuthorized().Data;
            var t=JsonConvert.DeserializeObject<Result>(JsonConvert.SerializeObject(result));
            Assert.IsTrue(t.value);
        }

        private class Result
        {
            public bool value { get; set; }
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public async Task ShouldFailConfirmUserIfTokenInModelNotSpecified()
        {
            await this.controller.ConfirmUser(new UserConfirmModel());
        }

        [Test]
        public async Task ShouldFailConfirmUserIfTokenInModelExpired()
        {
            var tokenInfo = GenerateTokenInfo(user.ID);
            user.Token = tokenInfo.Token;
            tokenInfo.TokenExpire = tokenInfo.TokenExpire.Value.AddDays(-1);
            controller.LoggedInUser = new LoggedInUserDetails(user);

            var result = await this.controller.ConfirmUser(new UserConfirmModel {Token = tokenInfo.TokenInfoEncoded});
            var viewResult = (ViewResult)result;

            // Assertion
            Assert.AreEqual(viewResult.ViewName, "EmailLinkExpired");
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public async Task ShouldFailConfirmUserIfTokenNotValid()
        {
            var tokenInfo = GenerateTokenInfo(user.ID);
            tokenInfo.TokenExpire = tokenInfo.TokenExpire.Value.AddDays(-1);
            controller.LoggedInUser = new LoggedInUserDetails(user);

            await this.controller.ConfirmUser(new UserConfirmModel { Token = tokenInfo.TokenInfoEncoded });
        }
                
        [Test]
        public async Task ShouldConfirmUser()
        {
            var tokenInfo = GenerateTokenInfo(user.ID);
            user.Token = tokenInfo.Token;
            user.TokenExpire = tokenInfo.TokenExpire;
            controller.LoggedInUser = new LoggedInUserDetails(user);

            await this.controller.ConfirmUser(new UserConfirmModel { Token = tokenInfo.TokenInfoEncoded});
            userService.Verify(i=>i.Update(It.IsAny<User>()));
        }

        [Test]
        public async Task ShouldConfirmUserAndNotifyAdminIfUserLegalOfficerInNewOrganization()
        {
            userService.Setup(m => m.GetLegalOfficers(organization.ID)).Returns(new List<User>());
            this.controller = new AccountController(userService.Object, organizationService.Object,
                configurationService.Object, notificationService.Object, authenticationManager.Object, identityServer.Object)
            {
                ControllerContext = context.Object,
                Url = mockUrl.Object,
                LoggedInUser = new LoggedInUserDetails(userSysAdmin)
            };

            var tokenInfo = GenerateTokenInfo(user.ID);
            user.Token = tokenInfo.Token;
            user.IsIntroducedAsLegalOfficer = true;
            user.TokenExpire = tokenInfo.TokenExpire;
            controller.LoggedInUser = new LoggedInUserDetails(user);

            await this.controller.ConfirmUser(new UserConfirmModel { Token = tokenInfo.TokenInfoEncoded });
            userService.Verify(i => i.Update(It.IsAny<User>()));
            userService.Verify(i => i.GetLegalOfficers(user.OrganizationID.Value));
            adminNotificationService.Verify(
                i =>
                    i.NewOrganizationInBackground(user.Name, organization.Name, It.IsAny<string>(),
                        It.IsAny<string>()));
            adminNotificationService.Verify(i => i.NewLegalOfficerInBackground(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Test]
        public async Task ShouldConfirmUserAndNotifyAdminIfUserLegalOfficer()
        {
            var tokenInfo = GenerateTokenInfo(user.ID);
            user.Token = tokenInfo.Token;
            user.TokenExpire = tokenInfo.TokenExpire;
            user.IsIntroducedAsLegalOfficer = true;
            controller.LoggedInUser = new LoggedInUserDetails(user);

            await this.controller.ConfirmUser(new UserConfirmModel { Token = tokenInfo.TokenInfoEncoded });
            userService.Verify(i => i.Update(It.IsAny<User>()));
            userService.Verify(i => i.GetLegalOfficers(user.OrganizationID.Value));
            adminNotificationService.Verify(i =>i.NewLegalOfficerInBackground(It.IsAny<string>(), It.IsAny<string>()));
        }

        private TokenInfo GenerateTokenInfo(int userId)
        {
            var expire = DateTime.Now.AddHours(1);

            // remove seconds 
            expire = expire.AddTicks(-(expire.Ticks % TimeSpan.TicksPerSecond));

            var id = BitConverter.GetBytes(userId);
            var time = BitConverter.GetBytes(expire.ToBinary());
            var guid = Guid.NewGuid();

            // concatinate generated data for activation link
            var idWithTime = id.Concat(time);
            var token = Convert.ToBase64String(idWithTime.Concat(guid.ToByteArray()).ToArray());

            return new TokenInfo
            {
                Token = guid.ToString(),
                TokenExpire = expire,
                TokenInfoEncoded = token
            };
        }
    }
}
