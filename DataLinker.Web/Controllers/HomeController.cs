using System.Web.Mvc;
using DataLinker.Services.Admin;
using DataLinker.Services.Organizations;
using DataLinker.Services.Authorisation;
using DataLinker.Models;

namespace DataLinker.Web.Controllers
{
    public class HomeController : BaseController
    {
        private readonly IOrganisationsService _organizationService;
        private readonly IAdminService _admin;

        public HomeController(IAuthorisationService auth,
            IOrganisationsService organisations,
            IAdminService admin)
            : base(auth)
        {
            _organizationService = organisations;
            _admin = admin;
        }

        [Authorize]
        [Route("home/index")]
        public ActionResult Index()
        {
            // Redirect admin to admin dashboard
            if (LoggedInUser.IsSysAdmin)
            {
                return RedirectToAction("AdminDashboard");
            }

            return RedirectToAction("Details", new { organizationId = LoggedInUser.Organization.ID });
        }

        [Authorize]
        [Route("organisations/{organizationId}/dashboard")]
        public ActionResult Details(int organizationId)
        {
            // Get model for dashboard
            DashboardModel model = _organizationService.SetupDashboardModel(organizationId, LoggedInUser);

            // Return result
            return View("Index", model);
        }
        
        [AllowAnonymous]
        public ActionResult About()
        {
            return View();
        }

        [Route("contact-us")]
        public ActionResult Contact()
        {
            ViewBag.Message = "Contact us";

            return View();
        }

        [Route("admin-dashboard")]
        public ActionResult AdminDashboard()
        {
            var model = _admin.GetModelForAdmin(LoggedInUser);
            return View(model);
        }
    }
}