using System;
using System.Web.Mvc;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using Rezare.AuditLog;
using DataLinker.Services.LicenseVerification;
using DataLinker.Services.Authorisation;
using DataLinker.Models;
using DataLinker.Services.OrganizationLicenses;
using DataLinker.Database.Models;

namespace DataLinker.Web.Controllers
{
    public class LicenseVerificationController : BaseController
    {
        private IAuditLogger _auditLog = AuditLogManager.GetAuditLogger();
        private readonly ILicenseVerificationService _licenseVerification;
        private readonly IOrganizationLicenseService _licenseService;

        public LicenseVerificationController(IAuthorisationService auth,
            ILicenseVerificationService licenseVerification,
            IOrganizationLicenseService licenseService)
            : base(auth)
        {
            _licenseService = licenseService;
            _licenseVerification = licenseVerification;
        }
                
        [Route("applications/{appId}/schemas/{schemaId}/license-verification")]
        public ActionResult ConfirmationScreen(string token)
        {
            try
            {
                // Get model
                LicenseConfirmModel model = _licenseVerification.GetConfirmModel(token, LoggedInUser);

                // Return appropriate view
                switch(model.Type)
                {
                    case OrganisationLicenseType.FromTemplate:
                        return View("ConfirmationScreen", model);

                    case OrganisationLicenseType.Custom:
                        return View("CustomLicenseVerification", model);

                    default:throw new BaseException("Unknown license type.");
                }
            }
            catch (BaseException ex)
            {
                // Log action
                _auditLog.Log(AuditStream.LegalAgreements, "Confirmation Failed: ",
                    new
                    {
                        id = LoggedInUser.ID,
                        email = LoggedInUser.Email
                    },
                    new
                    {
                        error = ex.Message,
                        remote_ip = Request.UserHostAddress,
                        browser = Request.Browser.Browser
                    });
                
                throw;
            }
        }
        
        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/approve")]
        public ActionResult Approve(int id, int appId, int schemaId)
        {
            // Setup url to licenses
            var urlToLicenseDetails = Url.Action("Index", "Licenses",
                new { appId, schemaId }, Request.Url.Scheme);

            // Approve organisation license
            _licenseVerification.Approve(id, urlToLicenseDetails, LoggedInUser);

            // Log action
            _auditLog.Log(AuditStream.LegalAgreements, "Approved",
                new
                {
                    licenseId = id,
                    id = LoggedInUser.ID,
                    email = LoggedInUser.Email
                },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });

            // Return view
            return View("LicenseVerificationThankYou");
        }

        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/decline")]
        public ActionResult Decline(int id, int appId, int schemaId)
        {
            // Setup url to licenses
            var urlToLicenseDetails = Url.Action("Index", "Licenses",
                new { appId, schemaId }, Request.Url.Scheme);

            // Update license
            _licenseVerification.Decline(id, urlToLicenseDetails, LoggedInUser);

            // Log action
            _auditLog.Log(AuditStream.LegalAgreements, "Declined",
                new
                {
                    licenseId = id,
                    id = LoggedInUser.ID,
                    email = LoggedInUser.Email
                },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });

            // Return view
            return View("LicenseVerificationThankYou");
        }

        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/consumerapprove")]
        public ActionResult ConsumerApprove(int id, int appId, int schemaId)
        {
            // Setup url to licenses
            var urlToLicenseDetails = Url.Action("Index", "Licenses",
                new { appId, schemaId }, Request.Url.Scheme);

            // Approve organisation license
            _licenseVerification.Approve(id, urlToLicenseDetails, LoggedInUser);

            // Log action
            _auditLog.Log(AuditStream.LegalAgreements, "Approved",
                new
                {
                    licenseId = id,
                    id = LoggedInUser.ID,
                    email = LoggedInUser.Email
                },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });

            // Return view
            return View("LicenseVerificationThankYou");
        }

        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/consumerdecline")]
        public ActionResult ConsumerDecline(int id, int appId, int schemaId)
        {
            // Setup url to licenses
            var urlToLicenseDetails = Url.Action("Index", "Licenses",
                new { appId, schemaId }, Request.Url.Scheme);

            // Update license
            _licenseVerification.Decline(id, urlToLicenseDetails, LoggedInUser);

            // Log action
            _auditLog.Log(AuditStream.LegalAgreements, "Declined",
                new
                {
                    licenseId = id,
                    id = LoggedInUser.ID,
                    email = LoggedInUser.Email
                },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });

            // Return view
            return View("LicenseVerificationThankYou");
        }

    }
}