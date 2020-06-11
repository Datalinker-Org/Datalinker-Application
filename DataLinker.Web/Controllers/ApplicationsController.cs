using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services.Applications;
using DataLinker.Services.Configuration;
using DataLinker.Services.Exceptions;
using PagedList;
using Rezare.AuditLog;
using DataLinker.Services.Authorisation;
using DataLinker.Models;
using DataLinker.Database.Models;

namespace DataLinker.Web.Controllers
{
    public class ApplicationsController : BaseController
    {
        private readonly IApplicationsService _applications;
        private readonly IConfigurationService _configuration;
        private readonly IAuditLogger _auditLog = AuditLogManager.GetAuditLogger();

        private DateTime GetDate => DateTime.UtcNow;

        public ApplicationsController(
            IApplicationsService applicationsService,
            IAuthorisationService auth,
            IConfigurationService configuration
            ) : base(auth)
        {
            _applications = applicationsService;
            _configuration = configuration;
        }
        
        [Route("applications")]
        public ActionResult Index(int? page)
        {
            // Get applications
            List<DataLinker.Models.ApplicationDetails> result =_applications.GetApplications(LoggedInUser);

            // Setup back navigation
            ViewBag.PreviousUrl = Url.Action("Index", "Home");
            var pageSize = _configuration.ManageApplicationsPageSize;
            var pageNumber = page ?? 1;
            return View(result.ToPagedList(pageNumber, pageSize));
        }
                
        [Route("applications/{id}")]
        public ActionResult Details(int id)
        {
            // Get application details model
            ApplicationDetails result = _applications.GetApplicationDetailsModel(id, LoggedInUser);

            // Format tokens for display
            foreach (var item in result.Hosts)
            {
                // Encode token
                item.Token = HttpUtility.UrlEncode(item.Token);
            }

            // Setup back url
            ViewBag.PreviousUrl = Url.Action("Index", "Home");

            // Return view
            return View(result);
        }
        
        [Route("applications/{id}/change-status/{value}")]
        public void ChangeStatus(int id, bool value)
        {
            // Update application
            _applications.UpdateStatus(id, value, LoggedInUser);
        }
        
        [AjaxOnly]
        [Route("applications/{appType}/create")]
        public ActionResult Create(string appType)
        {
            // Check whether organisation is active
            if (!LoggedInUser.Organization.IsActive)
            {
                throw new BaseException("Your organization is inactive. Please check if your organization has approved Legal Officer. For more details contact DataLinker administrator.");
            }

            // Setup application model
            var model = new Models.Applications.NewApplicationDetails();
            model.AppType = appType.Equals("service") ? "provider" : "consumer";
            model.IsProvider = appType.Equals("service");
            // Return result
            return PartialView("_Create",model);
        }

        [Route("applications/{appType}/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string appType, Models.Applications.NewApplicationDetails model)
        {
            // Setup url for notification
            var url = Url.Action("Index", "Applications", null, Request.Url.Scheme);

            // Setup name
            var data = (NewApplicationDetails)model;
            data.Name = model.Name;

            // Create application
            Application application = _applications.Create(url, model, LoggedInUser);

            // Log details for industry good application
            if (application.IsIntroducedAsIndustryGood)
            {
                // Log industry good action
                _auditLog.Log(AuditStream.General, "Industry Good",
                    new
                    {
                        id = LoggedInUser.ID,
                        email = LoggedInUser.Email
                    },
                    new
                    {
                        remote_ip = Request.UserHostAddress,
                        browser = Request.Browser.Browser
                    });
            }

            // Setup status message
            Toastr.Success("Service was successfully created");
            return RedirectToAction("Details", new { id = application.ID });
        }

        [Route("applications/{id}/edit")]
        public ActionResult Edit(int id)
        {
            // Get model
            var model = _applications.GetDetailsModelForEdit(id, LoggedInUser);

            Models.Applications.ApplicationDetails result = new DataLinker.Web.Models.Applications.ApplicationDetails
            {
                Name = model.Name,
                Description = model.Description,
                IsIntroducedAsIndustryGood = model.IsIntroducedAsIndustryGood
            };

            // Return result
            return PartialView("_Edit", result);
        }

        [Route("applications/{id}/edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, Models.Applications.ApplicationDetails appDetails)
        {
            var urlToApps = Url.Action("Index", "Applications", null, Request.Url.Scheme);

            // Setup name
            var model = (ApplicationDetails)appDetails;
            model.Name = appDetails.Name;

            // edit application
            Application application = _applications.EditApplication(id, urlToApps, model, LoggedInUser);

            // Log industry good action
            if (application.IsIntroducedAsIndustryGood && appDetails.IsIntroducedAsIndustryGood && !application.IsVerifiedAsIndustryGood)
            {
                _auditLog.Log(AuditStream.UserActivity, "Industry Good",
                    new
                    {
                        id = LoggedInUser.ID,
                        email = LoggedInUser.Email
                    },
                    new
                    {
                        remote_ip = Request.UserHostAddress,
                        browser = Request.Browser.Browser
                    });
            }

            // Setup status
            Toastr.Success("Application was successfully updated.");

            // Return result
            return RedirectToAction("Details", new { id = application.ID });
        }

        [AjaxOnly]
        [Route("applications/{id}/hosts")]
        public ActionResult GetApplicationHosts(int id)
        {
            // Get hosts for application
            List<ApplicationTokenDetails> result = _applications.GetHosts(id, LoggedInUser);

            // Encode all tokens
            foreach (var token in result)
            {
                token.Token = HttpUtility.UrlEncode(token.Token);
            }

            // Return result
            return PartialView("_ApplicationHosts", result);
        }

        [Route("applications/{id}/hosts/add")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewHost(int id, FormCollection formCollection)
        {
            // Get host value
            var host = formCollection["Host"];

            // Add host
            _applications.AddHost(id, host, LoggedInUser);

            // Setup default status message
            var msg = "New service host was successfully added";

            // Return result
            return Json(new { message = msg, isSuccess = true }, JsonRequestBehavior.AllowGet);
        }

        [Route("applications/{id}/hosts/{tokenId}/token")]
        public ActionResult GenerateNewToken(int id, int tokenId)
        {
            var newAppToken = _applications.CreateNewToken(id, tokenId, LoggedInUser);

            // Return details about generate token
            return Json(new
            {
                id = newAppToken.ID,
                url = Url.Action("GenerateNewToken", new { id = newAppToken.ApplicationID, tokenId = newAppToken.ID }),
                token = HttpUtility.UrlEncode(newAppToken.Token)
            },
                JsonRequestBehavior.AllowGet);
        }
        
        [Route("applications/{id}/hosts/{tokenId}")]
        [HttpDelete]
        public ActionResult ExpireToken(int id, int tokenId)
        {
            // Expire token
            _applications.ExpireAppToken(id, tokenId, LoggedInUser);

            // Return success response
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }
        
        [AjaxOnly]
        [Route("applications/{id}/authentication")]
        public ActionResult GetApplicationAuthentication(int id)
        {
            // Setup model
            var authDetails = _applications.GetApplicationAuthModel(id, LoggedInUser);

            // Return partial view
            return PartialView("_ApplicationAuthentication", authDetails);
        }
        
        [AjaxOnly]
        [Route("applications/{id}/authentication/edit")]
        public ActionResult EditApplicationAuthentication(int id)
        {
            // Setup authentication
            var result = _applications.SetupEditAppAuthModel(id, LoggedInUser);

            // Return result
            return PartialView("_EditApplicationAuthentication", result);
        }
        
        [Route("applications/{id}/authentication/edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditApplicationAuthentication(int id, ApplicationAuthenticationDetails model)
        {
            // Update authentication for user
            _applications.EditAuthentication(id, model, LoggedInUser);

            // Return result
            return RedirectToAction("Details", new { id });
        }

        [Route("applications/{id}/provider-endpoints/add")]
        public ActionResult AddProviderEndpoint(int id)
        {
            // Setup add provider endpoint model
            ProviderEndpointModel result = _applications.SetupAddProviderEndpointModel(id, LoggedInUser);

            // Return result
            return PartialView("_AddProviderEndpoint", result);

        }

        [Route("applications/{id}/provider-endpoints/add")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddProviderEndpoint(int id, ProviderEndpointModel model)
        {
            // Add endpoint
            _applications.AddEndpoint(id, model, LoggedInUser);

            // Setup status message
            Toastr.Success("Provider endpoint was successfully created! You can add data agreement for this schema now.");

            // Return result
            return RedirectToAction("Details", new { id });
        }

        [AjaxOnly]
        [Route("applications/{id}/provider-endpoints/{endpointId}/edit")]
        public ActionResult EditProviderEndpoint(int id, int endpointId)
        {
            // Setup model
            ProviderEndpointModel result = _applications.SetupProviderEndpointModel(id, endpointId, LoggedInUser);

            // Return result
            return PartialView("_EditProviderEndpoint", result);
        }

        [Route("applications/{id}/provider-endpoints/{endpointId}/edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProviderEndpoint(int id,int endpointId, ProviderEndpointModel model)
        {
            // Edit endpoint details
            ProviderEndpoint endpoint = _applications.EditEndpoint(id, endpointId, model, LoggedInUser);

            // Setup status
            Toastr.Success("Changes to endpoint were successfully saved.");

            // Return result
            return RedirectToAction("Details", new { endpointId = endpoint.ApplicationId });
        }
        
        [Route("applications/{id}/industry-good/approve")]
        public void ApproveIndustryGood(int id)
        {
            // Update application
            _applications.ApproveIndustryGoodRequest(id, LoggedInUser);
        }
        
        [Route("applications/{id}/industry-good/decline")]
        public void DeclineIndustryGood(int id)
        {
            // Update application
            _applications.DeclineIndustryGoodRequest(id, LoggedInUser);
        }
        
        [Route("applications/validate")]
        public JsonResult IsApplicationNotExistsForThisOrganization(string Name, string InitialName= null)
        {
            // Define result
            var result = _applications.IsApplicationExistsForThisOrganization(Name, InitialName, LoggedInUser);

            // Return result
            return Json(!result, JsonRequestBehavior.AllowGet);
        }
    }
}