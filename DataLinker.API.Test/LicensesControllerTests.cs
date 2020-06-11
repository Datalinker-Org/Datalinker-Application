using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Http.Hosting;
using DataLinker.API.Controllers;
using DataLinker.API.Models;
using DataLinker.Services.Applications;
using DataLinker.Services.Applications.Models;
using DataLinker.Services.Emails;
using DataLinker.Services.Emails.Roles.LegalOfficer;
using DataLinker.Models.Enums;
using DataLinker.Services.LicenseAgreements;
using DataLinker.Services.LicenseAgreements.Models;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Services.OrganizationLicenses.Models;
using DataLinker.Services.Organizations;
using DataLinker.Services.Organizations.Models;
using DataLinker.Services.Schemas;
using DataLinker.Services.Schemas.Models;
using DataLinker.Services.SoftwareStatements;
using Moq;
using NUnit.Framework;
using DataLinker.Services.LicenseMatches.Models;
using DataLinker.Services.Tests;

namespace DataLinker.API.Test
{
    [TestFixture]
    public class LicensesApiControllerTests
    {
        private Mock<IOrganizationService> _organizationService;
        private Mock<IApplicationsService> _applicationService;
        private Mock<IOrganizationLicenseService> _orgLicenseService;
        private Mock<IDataSchemaService> _dataSchemaService;
        private Mock<ILicenseAgreementService> _agreementService;
        private Mock<INotificationService> _notificationService;
        private Mock<ILegalOfficerNotificationService> _legalOfficerNotificationService;
        private Mock<ISchemaFileService> _schemaFileService;
        private Mock<ISoftwareStatementService> _softwareStatementService;
        private ILicenseMatchesService matchesService;
        private MockService<LicenseMatch> mService;

        private LicenseAgreementsController _controller;
        private LicenseDetails licenseDetails;
        private OrganizationLicense organizationLicense;
        private Mock<HttpRequestMessage> context;
        private string validSchema = "schema1";
        private string notValidSchema = "schema2";
        private string validStatement = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJzb2Z0d2FyZV9pZCI6ImI5NWM1NDllLWQ3MTEtNDU5Ny04OWUwLWZkYzMzZDFkMTdjYSIsImNsaWVudF9uYW1lIjoiRHVtbXkgQ29uc3VtZXIiLCJjbGllbnRfdXJpIjoiaHR0cHM6Ly93d3cuZGF0YWxpbmtlci5vcmcvZGV2L2FwcC8iLCJpc3MiOiI0MmUwMWYwOS0wZGVhLTQyYjItYjVlMS0wZjFlODc1NDczMjkiLCJzdWIiOiIxMGU1NzY2ZS1hZmYxLTRiMmUtYjkyNi0xYTFiNGNjZjU2NmMiLCJhdWQiOiJ1cm46b2F1dGg6c2NpbTpyZWc6Z2VuZXJpYyIsImV4cCI6IjE2MjA3ODM0MTQiLCJzY2hlbWFzIjoiW3tcInB1YmxpY19pZFwiOlwiOTFFOTI1Q0EtQjgzNi00QTMwLUI3RkEtMjc3QUNENDIwMjQ1XCIsXCJuYW1lXCI6XCJVYmVyIHNjaGVtYSB2MVwiLFwiZGVzY3JpcHRpb25cIjpcImRlc2NyXCIsXCJsaWNlbnNlSWRcIjoyMn0se1wicHVibGljX2lkXCI6XCJBNENBNjlGNS05RDNELTQ5MTktQkMzQi00NTczNjRDM0NGRjRcIixcIm5hbWVcIjpcIkNhcmNhc3MgZGF0YSB2MlwiLFwiZGVzY3JpcHRpb25cIjpcImRlc2NyXCIsXCJsaWNlbnNlSWRcIjoyM31dIn0.6mC3zBzIZK0srcmBvNTkra7vNP0qL0ipq5fuapdrkio";

        private string notValidStatement =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiYWRtaW4iOnRydWV9.TJVA95OrM7E2cBab30RMHrHDcEfxjoYZgeFONFh7HgQ";
        private DataSchema schema;
        private Application providerApplication;
        private Application consumerApplication;
        private Organization organization;
        private string applicationToken = "E86FA7A0-04EB-4B68-821E-7E221FEC4369";

        [SetUp]
        public void Init()
        {
            schema = new DataSchema
            {
                PublicID = validSchema,
                Status = (int)TemplateStatus.Active,
                ID = 1
            };
            organization = new Organization
            {
                ID = 1,
                Name = "Org",
                IsActive = true
            };
            providerApplication = new Application
            {
                PublicID = Guid.Parse("E86FA7A0-04EB-4B68-821E-7E221FEC4368"),
                ID = 1,
                Name = "application",
                IsProvider = true,
                OrganizationID = organization.ID
            };
            consumerApplication = new Application
            {
                PublicID = Guid.Parse("b95c549e-d711-4597-89e0-fdc33d1d17ca"),
                ID = 2,
                Name = "application",
                IsProvider = false,
                OrganizationID = organization.ID
            };
            organizationLicense = new OrganizationLicense
            {
                ApplicationID = providerApplication.ID,
                ID = 1,
                Status = (int)PublishStatus.Published,
                LicenseTemplateID = 1,
                ProviderEndpointID = 1,
                DataSchemaID = schema.ID
            };
            _organizationService = new Mock<IOrganizationService>();
            _applicationService = new Mock<IApplicationsService>();
            _orgLicenseService = new Mock<IOrganizationLicenseService>();
            _dataSchemaService = new Mock<IDataSchemaService>();
            _agreementService = new Mock<ILicenseAgreementService>();
            _notificationService = new Mock<INotificationService>();
            _schemaFileService = new Mock<ISchemaFileService>();
            _softwareStatementService = new Mock<ISoftwareStatementService>();
            _legalOfficerNotificationService = new Mock<ILegalOfficerNotificationService>();
            //Setup notification service
            _notificationService.SetupGet(i => i.LegalOfficer).Returns(_legalOfficerNotificationService.Object);

            // Setup Matches service
            mService = new MockService<LicenseMatch>();
            matchesService = new LicenseMatchesService(mService);
            mService.Add(new LicenseMatch { ConsumerLicenseID = organizationLicense.ID });
            mService.Add(new LicenseMatch { ProviderLicenseID = organizationLicense.ID, ConsumerLicenseID = organizationLicense.ID });

            // Setup license service
            _orgLicenseService.Setup(i => i.GetForApplicationAndSchema(providerApplication.ID, schema.ID, true))
                .Returns(new List<OrganizationLicense> {organizationLicense});
            _orgLicenseService.Setup(i => i.GetForApplicationAndSchema(consumerApplication.ID, schema.ID, true))
                .Returns(new List<OrganizationLicense> { organizationLicense });
            _orgLicenseService.Setup(i => i.GetForApplicationAndSchema(0, schema.ID, true)).Returns(new List<OrganizationLicense>());
            // Setup schemaFile
            _schemaFileService.Setup(i => i.Get(It.IsAny<Expression<Func<SchemaFile, bool>>>()))
                .Returns(new List<SchemaFile>
                {
                    new SchemaFile()
                });
            // Setup license agreement
            _agreementService.Setup(i => i.Add(It.IsAny<LicenseAgreement>())).Returns(true);
            // Setup application service
            _applicationService.Setup(i => i.Get(providerApplication.ID)).Returns(providerApplication);
            _applicationService.Setup(i => i.Get(consumerApplication.ID)).Returns(consumerApplication);
            _applicationService.Setup(i => i.Get(It.IsAny<Expression<Func<Application, bool>>>()))
                .Returns(new List<Application> {consumerApplication});
            // Setup data schema service
            _dataSchemaService.Setup(i => i.Get(validSchema)).Returns(schema);
            // Setup Statement
            _softwareStatementService.SetupGet(i => i.TokenValidationParameters).Returns(TokenValidationParameters);
            context = new Mock<HttpRequestMessage>();
            _controller = new LicenseAgreementsController(_organizationService.Object, _applicationService.Object,
                _orgLicenseService.Object, _dataSchemaService.Object, _agreementService.Object, _notificationService.Object,
                _schemaFileService.Object,_softwareStatementService.Object, matchesService)
            {
                Request = context.Object,
                LoggedInApp = new LoggedInApplication(providerApplication)
                {
                    TokenUsedToAuthorize = applicationToken,
                    Organization = new LoggedInOrganization(organization)
                }
            };

            _controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }
        
        [Test]
        public void ShouldFailCreateNewLicenseAgreement_ForInvalidSchema()
        {
            licenseDetails = new LicenseDetails { accepted_schemas = notValidSchema };
            var response = _controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public void ShouldFailCreateNewLicenseAgreement_ForInvalidSignature()
        {
            licenseDetails = new LicenseDetails { accepted_schemas = validSchema,software_statement = notValidStatement};
            var response = _controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public void ShouldFailCreateNewLicenseAgreement_ForInvalidConsumer()
        {
            _applicationService.Setup(i => i.Get(It.IsAny<Expression<Func<Application, bool>>>()))
                .Returns(new List<Application>());
            var controller = GetNewControllerObject();
            licenseDetails = new LicenseDetails { accepted_schemas = validSchema, software_statement = validStatement };
            var response = controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public void ShouldFailCreateNewLicenseAgreement_IfConsumerDoNotHaveLicense()
        {
            _orgLicenseService.Setup(i => i.GetForApplicationAndSchema(consumerApplication.ID, schema.ID, true))
                .Returns(new List<OrganizationLicense>());
            var controller = GetNewControllerObject();

            licenseDetails = new LicenseDetails { accepted_schemas = validSchema, software_statement = validStatement };
            var response = controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public void ShouldFailCreateNewLicenseAgreement_IfProviderDoNotHaveLicense()
        {
            _orgLicenseService.Setup(i => i.GetForApplicationAndSchema(providerApplication.ID, schema.ID, true))
                .Returns(new List<OrganizationLicense>());
            var controller = GetNewControllerObject();
            licenseDetails = new LicenseDetails { accepted_schemas = validSchema, software_statement = validStatement };
            var response = controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public void ShouldFailCreateNewLicenseAgreement_IfProviderLicensesDoNotMatch()
        {
            mService.Delete(i => i.ProviderLicenseID == organizationLicense.ID);
            var controller = GetNewControllerObject();
            licenseDetails = new LicenseDetails { accepted_schemas = validSchema, software_statement = validStatement };
            var response = controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.BadRequest);
        }

        [Test]
        public void ShouldNotCreateNewLicenseAgreement_IfAlreadyExists()
        {
            _agreementService.Setup(
                i => i.GetAgreementsForLicenseAndSchema(schema.ID, organizationLicense.ID, organizationLicense.ID))
                .Returns(new List<LicenseAgreement> {new LicenseAgreement()});
            var controller = GetNewControllerObject();
            licenseDetails = new LicenseDetails { accepted_schemas = validSchema, software_statement = validStatement };
            var response = controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            _agreementService.Verify(
                i => i.GetAgreementsForLicenseAndSchema(schema.ID, organizationLicense.ID, organizationLicense.ID));
        }

        [Test]
        public void ShouldCreateNewLicenseAgreement()
        {
            licenseDetails = new LicenseDetails { accepted_schemas = validSchema, software_statement = validStatement };
            var response = _controller.CreateNew(licenseDetails);
            Assert.IsTrue(response.StatusCode == HttpStatusCode.OK);
            _agreementService.Verify(i=>i.Add(It.IsAny<LicenseAgreement>()));
            _schemaFileService.Verify(i => i.Get(It.IsAny<Expression<Func<SchemaFile, bool>>>()));
            _legalOfficerNotificationService.Verify(
                i => i.LicenseAgreementCreatedInBackground(0, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        }

        private LicenseAgreementsController GetNewControllerObject()
        {
            var requestContext = new Mock<HttpRequestMessage>();
            var controller = new LicenseAgreementsController(_organizationService.Object, _applicationService.Object,
                _orgLicenseService.Object, _dataSchemaService.Object, _agreementService.Object, _notificationService.Object,
                _schemaFileService.Object, _softwareStatementService.Object, matchesService)
            {
                Request = requestContext.Object,
                LoggedInApp = new LoggedInApplication(providerApplication)
                {
                    TokenUsedToAuthorize = applicationToken,
                    Organization = new LoggedInOrganization(organization)
                }
            };
            controller.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
            return controller;
        }

        public TokenValidationParameters TokenValidationParameters => new TokenValidationParameters()
        {
            ValidAudiences = new string[]
            {
                "urn:oauth:scim:reg:generic"
            },
            ValidIssuers = new string[]
            {
                "42e01f09-0dea-42b2-b5e1-0f1e87547329"
            },
            IssuerSigningKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes("a1cb0802-b821-4f6e-a7d8-a1351e888c52"))
        };
    }
}
