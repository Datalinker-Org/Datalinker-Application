using DataLinker.Models.ConsumerProviderRegistration;
using DataLinker.Models.Enums;
using DataLinker.Services.Applications;
using DataLinker.Services.Authorisation;
using DataLinker.Services.ConsumerProviderRegistrations;
using DataLinker.Services.FileProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DataLinker.Web.Controllers
{
    public class ConsumerProviderRegistrationController : BaseController
    {
        private readonly IConsumerProviderRegistrationService _registrationService;
        private readonly ILicenseFileProvider _licenseService;

        public ConsumerProviderRegistrationController(IAuthorisationService auth, 
            IConsumerProviderRegistrationService registrationService,
            ILicenseFileProvider licenseService) : base(auth)
        {
            _registrationService = registrationService;
            _licenseService = licenseService;
        }

        // GET: Provider
        [Route("applications/{consumerAppId}/schemas/{schemaId}/consumer-provider-registration/providers")]
        public ActionResult Index(int consumerAppId, int schemaId)
        {
            var end = _registrationService.GetProvidersBySchemaId(consumerAppId, schemaId);
            ViewBag.PreviousUrl = Url.Action("Details", "Applications", new { id = consumerAppId });
            ViewBag.ConsumerAppId = consumerAppId;
            return View(end);
        }

        //public FileResult DownloadLicense(int appId, int schemaId, int licenseId)
        //{
        //    var result = _licenseService.GetLicenseForDownload(appId, schemaId, licenseId, LoggedInUser);
        //    return File(result.Content, result.MimeType, result.FileName);
        //}

        public ActionResult RequestProviderAccess(int consumerAppId, int providerLicenseId)
        {
            _registrationService.RequestForAccess(consumerAppId, providerLicenseId, LoggedInUser);
            return Json(new { message = "Request has been sent for an approval" }, JsonRequestBehavior.AllowGet);
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/consumer-legal-approval")]
        public ActionResult ConsumerLegalApprovalView(string token)
        {
            Log.Info($"Start ConsumerProviderRegistration - ConsumerLegalApprovalView(token: {token})");
            var legalApprovalModel = _registrationService.GetLegalApprovalModel(token, LoggedInUser);
            Log.Info($"Start ConsumerProviderRegistration - ConsumerLegalApprovalView(token: {token})");
            if (legalApprovalModel.Type == OrganisationLicenseType.FromTemplate)
            {
                return View("ConsumerLegalApprovalTemplateView", legalApprovalModel);
            }
            return View("ConsumerLegalApprovalCustomView", legalApprovalModel);
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/consumer-legal-approval/approve")]
        public ViewResult ConsumerLegalApprove(int consumerProviderRegistrationId)
        {
            Log.Info($"Start ConsumerProviderRegistration - ConsumerLegalApprove(consumerProviderRegistrationId: {consumerProviderRegistrationId})");
            try { 
                var model = _registrationService.ApproveByConsumerLegal(consumerProviderRegistrationId, LoggedInUser);
                ViewBag.PreviousUrl = Url.Action("Index", "ConsumerProviderRegistration", new { consumerAppId = model.ConsumerApplicationID, schemaID = model.SchemaID });
                Log.Info($"End ConsumerProviderRegistration - ConsumerLegalApprove(consumerProviderRegistrationId: {consumerProviderRegistrationId})");
                return View("ConsumerLegalApprovedView", model);
            } catch(Exception e)
            {
                Log.Error(e.ToString());
                throw e;
            }
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/consumer-legal-approval/decline-reason-popup")]
        public ActionResult ConsumerLegalDeclineReason(int consumerProviderRegistrationId)
        {
            LegalApprovalModel model = new LegalApprovalModel() {
                ConsumerProviderRegistrationID = consumerProviderRegistrationId,
                IsProvider = false
            };
            return PartialView("_DeclineReason", model);
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/consumer-legal-approval/decline")]
        public JsonResult ConsumerLegalDecline(int consumerProviderRegistrationId, string declineReason)
        {
            var model = _registrationService.DeclineByConsumerLegal(consumerProviderRegistrationId, declineReason, LoggedInUser);
            return Json(new { message = "Request has been declined.", model });
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/consumer-legal-approval/declined-view")]
        public ViewResult ConsumerLegalDeclinedView(int consumerProviderRegistrationId)
        {
            return View("ProviderLegalDeclinedView");
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/provider-legal-approval")]
        public ActionResult ProviderLegalApprovalView(int consumerProviderRegistrationId, string token)
        {
            var legalApprovalModel = new LegalApprovalModel();
            if (string.IsNullOrEmpty(token))
            {
                legalApprovalModel = _registrationService.GetLegalApprovalModel(consumerProviderRegistrationId, LoggedInUser);
            }
            else
            {
                legalApprovalModel = _registrationService.GetLegalApprovalModel(token, LoggedInUser);
            }

            if(legalApprovalModel.Type == OrganisationLicenseType.FromTemplate)
            {
                return View("ProviderLegalApprovalTemplateView", legalApprovalModel);
            }
            return View("ProviderLegalApprovalCustomView", legalApprovalModel);
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/provider-legal-approval/approve")]
        public ViewResult ProviderLegalApprove(int consumerProviderRegistrationId)
        {
            var model = _registrationService.ApproveByProviderLegal(consumerProviderRegistrationId, LoggedInUser);
            ViewBag.PreviousUrl = Url.Action("Details", "Applications", new { id = model.ProviderApplicationID });
            return View("ProviderLegalApprovedView", model);
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/provider-legal-approval/decline-reason-popup")]
        public ActionResult ProviderLegalDeclineReason(int consumerProviderRegistrationId)
        {
            var model = new LegalApprovalModel()
            {
                ConsumerProviderRegistrationID = consumerProviderRegistrationId,
                IsProvider = true
            };
            return PartialView("_DeclineReason", model);
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/provider-legal-approval/decline")]
        public JsonResult ProviderLegalDecline(int consumerProviderRegistrationId, string declineReason)
        {
            var model = _registrationService.DeclineByProviderLegal(consumerProviderRegistrationId, declineReason, LoggedInUser);
            return Json(new { message = "Request has been declined.", model });
        }

        [Route("consumer-provider-registration/{consumerProviderRegistrationId}/provider-legal-approval/declined-view")]
        public ViewResult ProviderLegalDeclinedView(int consumerProviderRegistrationId)
        {
            var model = _registrationService.GetConsumerProviderRegistrationDetail(consumerProviderRegistrationId, LoggedInUser);
            ViewBag.PreviousUrl = Url.Action("Details", "Applications", new { id = model.ProviderApplicationID });
            return View("ProviderLegalDeclinedView", model);
        }
    }
}