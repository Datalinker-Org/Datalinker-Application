using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using DataLinker.Services.Configuration;
using PagedList;
using DataLinker.Services.Organizations;
using DataLinker.Services.Authorisation;
using DataLinker.Models;

namespace DataLinker.Web.Controllers
{
    public class OrganizationController : BaseController
    {
        private readonly IOrganisationsService _organisations;
        private readonly IConfigurationService _configurationService;

        public OrganizationController(IAuthorisationService auth,
            IOrganisationsService orgService,
            IConfigurationService configurationService)
            : base(auth)
        {
            _organisations = orgService;
            _configurationService = configurationService;
        }

        [Route("organisations")]
        public ActionResult Index(int? page)
        {
            List<OrganizationModel> result =_organisations.GetOrganisationsModel(LoggedInUser);

            var pageSize = _configurationService.ManageOrganizationsPageSize;
            var pageNumber = page ?? 1;
            ViewBag.PreviousUrl = Url.Action("AdminDashboard", "Home");
            return View(result.ToPagedList(pageNumber, pageSize));
        }
        
        [Route("organisations/{id}/activate")]
        public ActionResult ChangeStatus(int id, bool value)
        {
            _organisations.UpdateStatus(id, value, LoggedInUser);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [AllowAnonymous]
        [Route("organisations/check-name")]
        public JsonResult IsOrganizationExists(string OrganizationName)
        {
            var result = _organisations.IsOrganisationNameUsed(OrganizationName);
            return Json(!result, JsonRequestBehavior.AllowGet);
        }
    }
}