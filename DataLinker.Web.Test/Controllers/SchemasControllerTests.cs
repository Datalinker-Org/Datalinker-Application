using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services.Applications;
using DataLinker.Services.Applications.Models;
using DataLinker.Services.Configuration;
using DataLinker.Services.Emails;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
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
using DataLinker.Web.Models.Schemas;
using DataLinker.Web.Models.Users;
using Moq;
using NUnit.Framework;
using PagedList;
using DataLinker.Services.LicenseMatches.Models;
using DataLinker.Services.Tests;

namespace DataLinker.Web.Test.Controllers
{
    [TestFixture]
    public class SchemasControllerTests
    {
        [SetUp]
        public void Init()
        {
            schemaModel = new SchemaModel
            {
                DataSchemaID = 1,
                Description = "Blah blah",
                Name = "Name",
                Status = TemplateStatus.Draft,
                Version = 1
            };

            dSchemaDraft = new DataSchema
            {
                ID = 1,
                Name = "Draft",
                CreatedBy = 1,
                CreatedAt = today,
                Status = (int) TemplateStatus.Draft
            };

            dSchemaPublished = new DataSchema
            {
                ID = 2,
                Name = "Published",
                CreatedAt = today,
                CreatedBy = 1,
                PublishedAt = today
            };

            dSchemaRetracted = new DataSchema
            {
                ID = 3,
                Name = "Retracted",
                CreatedAt = today,
                Status = (int)TemplateStatus.Retracted
            };

            schemaFile = new SchemaFile
            {
                DataSchemaID = dSchemaDraft.ID,
                CreatedBy = 1,
                FileFormat = ".xml",
                ID = 1
            };
            consumerOrganization = new Organization
            {
                ID=1
            };
            providerOrganization = new Organization
            {
                ID = 2
            };
            providerApplication = new Application
            {
                ID=1,
                OrganizationID = providerOrganization.ID
            };
            consumerApplication = new Application
            {
                ID = 2,
                OrganizationID = consumerOrganization.ID
            };
            providerLicense = new OrganizationLicense
            {
                ID = 1,
                ApplicationID = providerApplication.ID
            };
            consumerLicense = new OrganizationLicense
            {
                ID=2,
                ApplicationID = consumerApplication.ID
            };
            licenseMatch = new LicenseMatch
            {
                ID = 1,
                ConsumerLicenseID = consumerLicense.ID,
                ProviderLicenseID = providerLicense.ID
            };
            endpoint = new ProviderEndpoint
            {
                ApplicationId = providerApplication.ID,
                DataSchemaID = dSchemaPublished.ID,
                IsActive = true
            };

            var file = new Mock<HttpPostedFileBase>();
            file.Setup(i => i.InputStream).Returns(new MemoryStream());
            file.Setup(i => i.FileName).Returns("file.xml");
            schemaModel.UploadFile = file.Object;
            var mock = new Mock<UrlHelper>();
            // Setup file
            var fileMock = new Mock<HttpPostedFileBase>();
            fileMock.Setup(x => x.FileName).Returns("file1.xml");
            var context = new Mock<ControllerContext>();

            context.Setup(m => m.HttpContext.Request.Files.Count).Returns(1);
            context.Setup(m => m.HttpContext.Request.Files[0]).Returns(fileMock.Object);
            context.Setup(m => m.HttpContext.Request.Url).Returns(new Uri("http://test.com"));
            sysAdmin = new User
            {
                ID = 1,
                IsActive = true,
                Email = "email@mail.com",
                IsSysAdmin = true
            };

            // Setup Services
            metaDataService = new Mock<IDataSchemaService>();
            fileService = new Mock<ISchemaFileService>();
            organizationService = new Mock<IOrganizationService>();
            userService = new Mock<IUserService>();
            notificationService = new Mock<INotificationService>();
            endpointService = new Mock<IProviderEndpointService>();
            applicationService = new Mock<IApplicationsService>();
            configurationService = new Mock<IConfigurationService>();
            organizationLicenseService = new Mock<IOrganizationLicenseService>();

            userService.Setup(u => u.Get(sysAdmin.ID)).Returns(sysAdmin);
            configurationService.SetupProperty(p => p.ManageSchemasPageSize,5);
            // Setup organization service
            organizationService.Setup(m => m.Get(consumerOrganization.ID)).Returns(consumerOrganization);
            organizationService.Setup(m => m.Get(providerOrganization.ID)).Returns(providerOrganization);
            // Setup application service
            applicationService.Setup(m => m.Get(providerApplication.ID)).Returns(providerApplication);
            applicationService.Setup(m => m.Get(consumerApplication.ID)).Returns(consumerApplication);
            // Setup endpoint service
            endpointService.Setup(m => m.Get(endpoint.ID)).Returns(endpoint);
            // Setup organization licenses
            organizationLicenseService.Setup(i => i.Get(It.IsAny<Expression<Func<OrganizationLicense, bool>>>()))
                .Returns(new List<OrganizationLicense>());
            organizationLicenseService.Setup(m => m.GetAllProviderLicensesForMonth(It.IsAny<DateTime>()))
                .Returns(new List<OrganizationLicense> {providerLicense});
            organizationLicenseService.Setup(m => m.Get(consumerLicense.ID)).Returns(consumerLicense);
            // Schema file service
            fileService.Setup(m => m.Add(It.IsAny<SchemaFile>())).Returns(true);
            fileService.Setup(m => m.Update(It.IsAny<SchemaFile>())).Returns(true);
            fileService.Setup(m => m.Get(schemaFile.ID)).Returns(schemaFile);
            fileService.Setup(m => m.Get(It.IsAny<Expression<Func<SchemaFile, bool>>>()))
                .Returns(new List<SchemaFile> {schemaFile});
            // Dataschema service
            metaDataService.Setup(m => m.GetAllSchemas(1, false)).Returns(new List<DataSchema> { dSchemaDraft });
            metaDataService.Setup(m => m.Add(It.IsAny<DataSchema>())).Returns(true);
            metaDataService.Setup(m => m.Update(It.IsAny<DataSchema>())).Returns(true);
            metaDataService.Setup(m => m.Get(dSchemaDraft.ID)).Returns(dSchemaDraft);
            metaDataService.Setup(m => m.Get(dSchemaPublished.ID)).Returns(dSchemaPublished);
            metaDataService.Setup(m => m.Get(dSchemaRetracted.ID)).Returns(dSchemaRetracted);

            // License matches
            var mService = new MockService<LicenseMatch>();
            matchesService = new LicenseMatchesService(mService);
            matchesService.Add(licenseMatch);

            // Setup controller
            controller = new SchemasController(metaDataService.Object, fileService.Object, userService.Object,
                organizationService.Object,endpointService.Object,applicationService.Object,notificationService.Object,organizationLicenseService.Object, matchesService,configurationService.Object)
            {
                ControllerContext = context.Object,
                LoggedInUser = new LoggedInUserDetails(sysAdmin),
                Url = mock.Object
            };
        }

        private ILicenseMatchesService matchesService { get; set; }

        private SchemasController controller { get; set; }
        private Mock<IDataSchemaService> metaDataService { get; set; }
        private Mock<IOrganizationLicenseService> organizationLicenseService { get; set; }
        private Mock<IUserService> userService { get; set; }
        private Mock<IProviderEndpointService> endpointService { get; set; }
        private Mock<IApplicationsService> applicationService { get; set; }
        private Mock<INotificationService> notificationService { get; set; }
        private Mock<IOrganizationService> organizationService { get; set; }
        private Mock<ISchemaFileService> fileService { get; set; }
        private Mock<IConfigurationService> configurationService { get; set; }
        private DataSchema dSchemaDraft { get; set; }
        private DataSchema dSchemaPublished { get; set; }
        private DataSchema dSchemaRetracted { get; set; }
        private SchemaModel schemaModel { get; set; }
        private SchemaFile schemaFile { get; set; }
        private OrganizationLicense consumerLicense { get; set; }
        private OrganizationLicense providerLicense { get; set; }

        private ProviderEndpoint endpoint { get; set; }

        private Application providerApplication { get; set; }
        private Application consumerApplication { get; set; }
        private Organization providerOrganization { get; set; }
        private Organization consumerOrganization { get; set; }
        private LicenseMatch licenseMatch { get; set; }

        private User sysAdmin { get; set; }

        private DateTime today = DateTime.Now;
        
        [Test]
        public void ShouldAddNewSchemaFileAndNewMetaData()
        {
            schemaModel.DataSchemaID = 0;
            var result = (RedirectToRouteResult) controller.Create(schemaModel);
            metaDataService.Verify(m => m.Add(It.IsAny<DataSchema>()));
            fileService.Verify(m => m.Add(It.IsAny<SchemaFile>()));
            Assert.IsTrue(result.RouteValues["action"].ToString() == "Index");
        }
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanManageSchemas()
        {
            controller.LoggedInUser = new LoggedInUserDetails(new User {ID = 2,IsSysAdmin = false});
            controller.Index(null);
        }

        [Test]
        public void ShouldPublishDraftSchema()
        {
            var resultStatus = controller.PublishSchema(dSchemaDraft.ID);
            metaDataService.Verify(m => m.Get(dSchemaDraft.ID));
            metaDataService.Verify(m => m.Update(dSchemaDraft));
            Assert.IsTrue(dSchemaDraft.IsActive);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanPublishSchema()
        {
            controller.LoggedInUser = new LoggedInUserDetails(new User {ID = 9, IsSysAdmin = false});
            controller.PublishSchema(dSchemaPublished.ID);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanRetractSchema()
        {
            controller.LoggedInUser = new LoggedInUserDetails(new User { ID = 9, IsSysAdmin = false });
            controller.RetractSchema(dSchemaPublished.ID);
        }

        [Test]
        public void ShouldRetractPublishedSchema()
        {
            var resultStatus = controller.RetractSchema(dSchemaPublished.ID);
            metaDataService.Verify(m => m.Get(dSchemaPublished.ID));
            dSchemaDraft.RetractedAt = today;
            metaDataService.Verify(m => m.Update(It.IsAny<DataSchema>()));
            Assert.IsTrue(dSchemaPublished.IsRetracted);
        }

        [Test]
        public void ShouldReturModelForCreate()
        {
            var result = (ViewResult) controller.Create();
            Assert.IsTrue(result.Model != null);
        }

        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldFailReturModelForCreateForNotAdmin()
        {
            controller.LoggedInUser = new LoggedInUserDetails(new User {IsSysAdmin = false });
            controller.Create();
        }

        [Test]
        public void ShouldReturnCorrectModelForEditView()
        {
            var result = (ViewResult) controller.Edit(dSchemaDraft.ID);
            var model = (SchemaModel) result.Model;
            Assert.AreEqual(dSchemaDraft.ID, model.DataSchemaID);
            Assert.AreEqual(dSchemaDraft.Name, model.Name);
            Assert.AreEqual((TemplateStatus)dSchemaDraft.Status, model.Status);
        }
        
        [Test]
        public void ShouldReturnFileForSchema()
        {
            var result = controller.Download(schemaFile.ID);
            Assert.IsTrue(result.FileDownloadName == dSchemaDraft.Name + schemaFile.FileFormat);
        }

        [Test]
        public void ShouldReturnListOfSchemas()
        {
            var result = (ViewResult) controller.Index(null);
            var model = (IPagedList<SchemaModel>) result.Model;
            metaDataService.Verify(m => m.GetAllSchemas(1, false));
            Assert.IsTrue(model.Count == 1 && model[0].Name != null);
            Assert.NotNull(model);
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void ShouldThrowErrorForInvalidFileFormat()
        {
            var file = new Mock<HttpPostedFileBase>();
            file.Setup(i => i.InputStream).Returns(new MemoryStream());
            file.Setup(i => i.FileName).Returns("file.txt");
            schemaModel.UploadFile = file.Object;
            
            controller.Create(schemaModel);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void ShouldThrowExceptionWhenEditRetracted()
        {
            controller.Edit(dSchemaRetracted.ID);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void ShouldThrowExceptionWhenUpdateRetractedSchema()
        {
            schemaModel.DataSchemaID = dSchemaRetracted.ID;
            controller.Edit(dSchemaRetracted.ID, schemaModel);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void ShouldThrowIdException()
        {
            controller.Download(null);
        }

        [Test]
        [ExpectedException(typeof (BaseException))]
        public void ShouldThrowSchemaFileException()
        {
            controller.Download(30);
        }

        [Test]
        public void ShouldUpdateDescriptionOfPublishedSchema()
        {
            schemaModel.DataSchemaID = 1;
            controller.Edit(schemaModel.DataSchemaID, schemaModel);
            metaDataService.Verify(m => m.Update(It.IsAny<DataSchema>()));
        }

        [Test]
        public void ShouldUpdateFileAndUpdateDataSchemaWithSchemaFile()
        {
            controller.Edit(1,schemaModel);
            fileService.Verify(m => m.Get(It.IsAny<Expression<Func<SchemaFile, bool>>>()));
            fileService.Verify(m => m.Update(It.IsAny<SchemaFile>()));
        }

        [Test]
        public void ShouldUpdateSchemaWithMetaDataAndUpdateSchemaFile()
        {
            schemaModel.DataSchemaID = 1;
            var result = (RedirectToRouteResult) controller.Edit(schemaModel.DataSchemaID, schemaModel);
            metaDataService.Verify(m => m.Get(It.IsAny<int>()));
            fileService.Verify(m => m.Get(It.IsAny<Expression<Func<SchemaFile, bool>>>()));
            metaDataService.Verify(m => m.Update(It.IsAny<DataSchema>()));
            fileService.Verify(m => m.Update(It.IsAny<SchemaFile>()));
            Assert.IsTrue(result.RouteValues["action"].ToString() == "Index");
        }
        
        [Test]
        [ExpectedException(typeof(BaseException))]
        public void OnlyAdminCanGenerateReport()
        {
            controller.LoggedInUser = new LoggedInUserDetails(new User { IsSysAdmin = false});
            controller.GenerateReport();
        }

        [Test]
        public void ShouldGenerateReport()
        {
            controller.GenerateReport();
        }
    }
}