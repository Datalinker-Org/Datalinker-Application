using System;
using System.Web.Mvc;
using DataLinker.Services.Authorisation;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Services.Schemas;
using DataLinker.Services.SoftwareStatements;

namespace DataLinker.Web.Controllers
{
    public class SoftwareStatementController : BaseController
    {
        private DateTime GetDate=> DateTime.Now;
        private readonly ISoftwareStatementService _softwareStatementService;
        private readonly IOrganizationLicenseService _organizationLicenseService;
        private readonly IDataSchemaService _dataSchemaService;

        public SoftwareStatementController(IAuthorisationService auth,
            ISoftwareStatementService softwareStatementService,
            IDataSchemaService dataSchemaService,
            IOrganizationLicenseService organizationLicenseService) : base(auth)
        {
            _softwareStatementService = softwareStatementService;
            _dataSchemaService = dataSchemaService;
            _organizationLicenseService = organizationLicenseService;
        }
        
        [AjaxOnly]
        [Route("applications/{applicationId}/software-statement")]
        public ActionResult Get(int applicationId)
        {
            var validStatement = _softwareStatementService.GetValidStatement(applicationId, LoggedInUser, LoggedInUser.Organization.ID);

            return PartialView("_Details", validStatement.Content);
        }

        [HttpPost]
        [Route("applications/{applicationId}/software-statement")]
        // todo Anti forgery
        //[ValidateAntiForgeryToken]
        public ActionResult GenerateNew(int applicationId)
        {
            var validStatement = _softwareStatementService.UpdateSoftwareStatement(applicationId, LoggedInUser, LoggedInUser.Organization.ID);
            return Json(new { newStatement = validStatement.Content }, JsonRequestBehavior.AllowGet);
        }
    }
}