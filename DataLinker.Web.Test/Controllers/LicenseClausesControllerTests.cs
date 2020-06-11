using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services;
using DataLinker.Services.Emails;
using DataLinker.Services.Emails.Roles.User;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseTemplates;
using DataLinker.Services.LicenseTemplates.Models;
using DataLinker.Services.Organizations;
using DataLinker.Services.Users;
using DataLinker.Services.Users.Models;
using DataLinker.Web.Controllers;
using DataLinker.Web.Models.Licenses;
using DataLinker.Web.Models.Users;
using Moq;
using NUnit.Framework;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class LicenseClausesControllerTests
    {

        private Mock<ILicenseClauseService> _clauseService;
        private Mock<ILicenseClauseTemplateService> _clauseTemplateService;
        private Mock<ILicenseSectionService> _sectionService;
        private Mock<IOrganizationService> organizationService;
        private Mock<IUserService> userService;
        private Mock<ILicenseTemplatesService> licenseTemplateService;
        private Mock<INotificationService> notificationService;
        private Mock<IUserNotificationService> userNotificationService;
        private Mock<ControllerContext> context;

        private LicenseClausesController controller;
        private User userSysAdmin;
        private User user;
        private LicenseSection sectionA;
        private LicenseSection sectionB;
        private LicenseSection sectionC;
        private LicenseClause clause1SectionA;
        private LicenseClause clause2SectionA;
        private LicenseClause clause1SectionB;
        private LicenseTemplate licenseTemplate;
        private LicenseClauseTemplate clauseTemplateDraft;
        private LicenseClauseTemplate clauseTemplateActive;

        [SetUp]
        public void Init()
        {
            var today = DateTime.Now;
            userSysAdmin = new User
            {
                ID = 2,
                OrganizationID = 1,
                IsActive = true,
                Email = "Email@mail.com",
                IsSysAdmin = true,
            };

            user = new User
            {
                ID = 3,
                IsActive = true,
                Email = "Email@mail.com",
                IsSysAdmin = false,
                OrganizationID = null,
            };

            sectionA = new LicenseSection
            {
                ID = 1,
                CreatedBy = user.ID,
                Title = "sectionA"
            };

            sectionB = new LicenseSection
            {
                ID = 2,
                CreatedBy = user.ID,
                Title = "sectionB"
            };

            sectionC = new LicenseSection
            {
                ID = 3,
                CreatedBy = user.ID,
                Title = "sectionC"
            };

            licenseTemplate = new LicenseTemplate
            {
                ID = 1,
                CreatedAt = today,
                CreatedBy = 1,
                Status = (int)TemplateStatus.Active,
                Name = "titleLicense"
            };

            clauseTemplateDraft = new LicenseClauseTemplate
            {
                ID = 1,
                Status = (int) TemplateStatus.Draft,
                Description = "Descr",
                LegalText = "text",
                ShortText = "shortText",
                LicenseClauseID = 1,
                Version = 1
            };

            clauseTemplateActive = new LicenseClauseTemplate
            {
                ID = 2,
                Status = (int)TemplateStatus.Active,
                Description = "Descr",
                LegalText = "text",
                ShortText = "shortText",
                Version = 1
            };

            clause1SectionA = new LicenseClause
            {
                ID = 1,
                OrderNumber = 0,
                LicenseSectionID = sectionA.ID
                };

            clause2SectionA = new LicenseClause
            {
                ID = 2,
                OrderNumber = 0,
                LicenseSectionID = sectionA.ID
            };

            clause1SectionB = new LicenseClause
            {
                ID = 3,
                OrderNumber = 0,
                LicenseSectionID = sectionA.ID
            };
            
            var fileMock = new Mock<HttpPostedFileBase>();
            _clauseService = new Mock<ILicenseClauseService>();
            _clauseTemplateService = new Mock<ILicenseClauseTemplateService>();
            _sectionService = new Mock<ILicenseSectionService>();
            organizationService = new Mock<IOrganizationService>();
            userService = new Mock<IUserService>();
            licenseTemplateService = new Mock<ILicenseTemplatesService>();
            userNotificationService = new Mock<IUserNotificationService>();
            notificationService = new Mock<INotificationService>();
            // Setup notification service
            notificationService.SetupGet(m => m.User).Returns(userNotificationService.Object);
            var urlHelper = new Mock<UrlHelper>();
            context = new Mock<ControllerContext>();
            context.Setup(m => m.HttpContext.Request.Files.Count).Returns(1);
            context.Setup(m => m.HttpContext.Request.Files[0]).Returns(fileMock.Object);

            _sectionService.Setup(m => m.GetAll()).Returns(new List<LicenseSection> {sectionA, sectionB, sectionC});
            _sectionService.Setup(m => m.Get(sectionA.ID)).Returns(sectionA);
            _sectionService.Setup(m => m.Get(sectionB.ID)).Returns(sectionB);
            _sectionService.Setup(m => m.Get(sectionC.ID)).Returns(sectionC);

            _clauseService.Setup(m => m.Get(It.IsAny<Expression<Func<LicenseClause, bool>>>()))
                .Returns(new List<LicenseClause> {clause1SectionA, clause2SectionA});
            _clauseService.Setup(i => i.Get(clause1SectionA.ID)).Returns(clause1SectionA);

            _clauseTemplateService.Setup(m => m.Get(It.IsAny<Expression<Func<LicenseClauseTemplate, bool>>>()))
                .Returns(new List<LicenseClauseTemplate> {clauseTemplateDraft});
            _clauseTemplateService.Setup(m => m.Get(clauseTemplateDraft.ID)).Returns(clauseTemplateDraft);
            _clauseTemplateService.Setup(m => m.Get(clauseTemplateActive.ID)).Returns(clauseTemplateActive);

            licenseTemplateService.Setup(m => m.GetPublishedGlobalLicense()).Returns(licenseTemplate);

            controller = new LicenseClausesController(_clauseService.Object, _sectionService.Object,
                _clauseTemplateService.Object, userService.Object, organizationService.Object,
                licenseTemplateService.Object, notificationService.Object)
            {
                LoggedInUser = new LoggedInUserDetails(userSysAdmin),
                Url = urlHelper.Object,
                ControllerContext = context.Object
            };
        }

        [Test]
        public void ShouldReturnListOfClauseTemplates()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            var viewResult = (ViewResult)controller.Index();
            var model = (SectionsAndClausesModel)viewResult.Model;
            Assert.IsNotNull(model.Sections);
            Assert.IsNotNull(model);
            Assert.IsNotNull(model.GlobalLicense);
            Assert.IsTrue(model.Sections.First().ClauseTemplates.Count > 0);
        }

        [Test]
        public void ShouldReturnListOfSectionsForClause()
        {
            controller.LoggedInUser = new LoggedInUserDetails(userSysAdmin);
            var viewResult = (ViewResult)controller.Index();
            var model = (SectionsAndClausesModel)viewResult.Model;
            licenseTemplateService.Verify(m=>m.GetPublishedGlobalLicense());
            _sectionService.Verify(m=>m.GetAll());
            Assert.IsNotNull(model.Sections);
            Assert.IsTrue(model.Sections.Count > 0);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanAccessListTemplates()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Index();
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanAccessCreateNewClauseTemplateForSection()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Create(new LicenseClauseTemplateModel());
        }

        [Test]
        public void ShouldCreateNewClauseTemplateForSection()
        {
            var viewResult = (ViewResult)controller.Create(sectionA.ID);
            var model = (LicenseClauseTemplateModel)viewResult.Model;
            model.ShortText = clauseTemplateDraft.ShortText;
            model.Description = clauseTemplateDraft.Description;
            model.LegalText = clauseTemplateDraft.LegalText;
            controller.Create(model);
            _clauseTemplateService.Verify(m=>m.Add(It.IsAny<LicenseClauseTemplate>()));
            _clauseService.Verify(m=>m.Add(It.IsAny<LicenseClause>()));
        }

        [Test]
        public void ShouldFailReturnCreateNewClauseTemplateForNotExistingSection()
        {
            var result = (HttpStatusCodeResult)controller.Create(0);
            Assert.IsTrue(result.StatusCode == (int)HttpStatusCode.NotFound);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void OnlyAdminCanCreateClauseTemplateForSection()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Create(1);
        }

        [Test]
        public void ShouldEditClauseTemplateForSection()
        {
            var viewResult = (ViewResult)controller.Edit(clauseTemplateDraft.ID);
            var model = (LicenseClauseTemplateModel) viewResult.Model;
            Assert.IsNotNull(model.ID);
            _clauseTemplateService.Verify(m=>m.Get(clauseTemplateDraft.ID));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventEditActiveClauseTemplateForSection()
        {
            var viewResult = (ViewResult)controller.Edit(clauseTemplateActive.ID);
            var model = (LicenseClauseTemplateModel)viewResult.Model;
            Assert.IsNotNull(model.ID);
            _clauseTemplateService.Verify(m => m.Get(clauseTemplateActive.ID));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanSaveChangesFOrClauseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Edit(clauseTemplateActive.ID, new LicenseClauseTemplateModel(clauseTemplateActive));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldPreventSaveChangesForActiveClauseTemplate()
        {
            controller.Edit(clauseTemplateActive.ID,new LicenseClauseTemplateModel(clauseTemplateActive));
            _clauseTemplateService.Verify(m => m.Get(clauseTemplateActive.ID));
        }

        [Test]
        public void ShouldSaveChangesForDraftClauseTemplate()
        {
            controller.Edit(clauseTemplateDraft.ID, new LicenseClauseTemplateModel(clauseTemplateDraft));
            _clauseTemplateService.Verify(m => m.Get(clauseTemplateDraft.ID));
            _clauseTemplateService.Verify(m => m.Update(It.IsAny<LicenseClauseTemplate>()));
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanEditClauseTemplate()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Edit(1);
        }

        [Test]
        public void ShouldUploadFileWithClauseTemplates()
        {
            controller.Upload(sectionA.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanUploadClauseTemplates()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Upload(1);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanPublishClauseTemplates()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Publish(clauseTemplateDraft.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailPublishNotDraftClauseTemplates()
        {
            controller.Publish(clauseTemplateActive.ID);
        }

        [Test]
        public void ShouldPublishDraftClauseTemplates()
        {
            controller.Publish(clauseTemplateDraft.ID);
            userNotificationService.Verify(i=>i.NewClauseInBackground(clauseTemplateDraft.ID));
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanRetractClauseTemplates()
        {
            controller.LoggedInUser = new LoggedInUserDetails(user);
            controller.Retract(clauseTemplateDraft.ID);
        }

        [Test]
        public void ShouldRetractDraftClauseTemplates()
        {
            controller.Retract(clauseTemplateDraft.ID);
        }

        [Test]
        public void ShouldRetractActiveClauseTemplates()
        {
            controller.Retract(clauseTemplateActive.ID);
        }
    }
}
