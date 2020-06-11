using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DataLinker.Services.Configuration;
using DataLinker.Services.Exceptions;
using DataLinker.Services.OrganizationLicenses;
using PagedList;
using Rezare.AuditLog;
using System.Web;
using DataLinker.Services.CustomLicenses;
using DataLinker.Services.FileProviders;
using System.Collections.Generic;
using DataLinker.Services.Authorisation;
using DataLinker.Models;
using DataLinker.Web.Models.Licenses;

namespace DataLinker.Web.Controllers
{
    public class LicensesController : BaseController
    {
        private readonly IAuditLogger _auditLog = AuditLogManager.GetAuditLogger();
        private readonly IOrganizationLicenseService _licenseService;
        private readonly IConfigurationService _config;
        private readonly ICustomLicenseService _customLicenses;
        private readonly ILicenseFileProvider _licenseFiles;
        private readonly ILicenseComparerService _licenseComparer;

        public LicensesController(IAuthorisationService auth,
            IOrganizationLicenseService licenseService,
            ICustomLicenseService customLicenses,
            ILicenseFileProvider licenseFiles,
            ILicenseComparerService licenseComparer,
            IConfigurationService config)
            : base(auth)
        {
            _licenseService = licenseService;
            _config = config;
            _customLicenses = customLicenses;
            _licenseFiles = licenseFiles;
            _licenseComparer = licenseComparer;
        }

        private DateTime GetDate => DateTime.UtcNow;

        [Route("applications/{appId}/schemas/{schemaId}/licenses")]
        public ActionResult Index(int appId, int schemaId, int? page)
        {
            // Get model with licenses
            var model = _licenseService.SetupProviderLicensesModel(appId, schemaId, LoggedInUser);

            // Setup result view model
            var result = new Models.Licenses.ProviderLicensesModel
            {
                AnyInVerificationProcess = model.AnyInVerificationProcess,
                AnyPublished = model.AnyPublished,
                IsProvider = model.IsProvider,
                SchemaName = model.SchemaName
            };

            // Setup previous url
            ViewBag.PreviousUrl = Url.Action("Details", "Applications", new { id = appId });

            // Get pagesize
            var pageSize = _config.ManageLicensesPageSize;

            // Setup page number
            var pageNumber = page ?? 1;

            // Apply default ordering
            result.Items = model.Items.OrderByDescending(p => p.CreatedAt).ToPagedList(pageNumber, pageSize);

            // Return view
            return View(result);
        }

        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/publish")]
        public ActionResult Publish(int id, int schemaId, int appId)
        {
            // Publish license
            _licenseService.Publish(id, appId, schemaId, LoggedInUser);

            // Setup status message
            Toastr.Success("License was successfully published.");

            // Setup redirect url
            var redirectUrl = Url.Action("Index", "Licenses", new { appId, schemaId });

            // Return result
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }
        
        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/retract")]
        public ActionResult Retract(int id, int schemaId, int appId)
        {
            // Retract license
            _licenseService.Retract(id, LoggedInUser);

            // Setup redirect url
            var redirectUrl = Url.Action("Index", "Licenses", new { appId, schemaId });

            // Return result
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }

        [Route("applications/{appId}/schemas/{schemaId}/provider-licenses/{id}/download")]
        public FileResult Download(int id, int schemaId, int appId)
        {
            // Get result
            var result = _licenseFiles.GetLicenseForDownload(appId, schemaId, id, LoggedInUser);
            
            // Return file
            return File(result.Content, result.MimeType, result.FileName);
        }

        [Route("applications/{appId}/schemas/{schemaId}/provider-licenses/{id}/downloadview")]
        public ActionResult DownloadView(int id, int schemaId, int appId)
        {
            var model = new LicenseDownloadVm()
            {
                ApplicationId = appId,
                SchemaId = schemaId,
                LicenseId = id,
            };
            return View(model);
        }

        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/draft")]
        public ActionResult MoveToDraft(int id, int schemaId, int appId)
        {
            // Update license
            _licenseService.Draft(id, LoggedInUser);

            // Setup status message
            Toastr.Info("License was moved to draft");

            // Return result
            return RedirectToAction("Index", "Licenses", new { schemaId, appId });
        }

        [Route("applications/{appId}/schemas/{schemaId}/licenses/{id}/request-verification")]
        public ActionResult SendToLegalOfficer(int id, int schemaId, int appId)
        {
            // Send request
            _licenseService.RequestLicenseVerification(id, appId, schemaId, LoggedInUser);
            
            // Log user action
            _auditLog.Log(AuditStream.LegalAgreements, "Send to Legal", new { LoggedInUser.ID, LoggedInUser.Email },
                new
                {
                    remote_ip = Request.UserHostAddress,
                    browser = Request.Browser.Browser
                });

            // Redirect to provider licenses screen
            var redirectUrl = Url.Action("Index", "Licenses", new { appId, schemaId });

            Toastr.Success("License was sent to Legal Officer of your organization.");
            return Json(new { Url = redirectUrl, isSuccess = true }, JsonRequestBehavior.AllowGet);
        }

        [Route("license-agreements/{id}/download")]
        public FileResult DownloadAgreement(int id)
        {
            // Get file result
            var fileResult = _licenseFiles.GetAgreement(id, LoggedInUser);

            // Setup file result
            var stream = new MemoryStream(fileResult.Content);

            // Return result
            return File(stream, fileResult.MimeType, fileResult.FileName);
        }
                                
        [Route("applications/{appId}/schemas/{schemaId}/licenses/create")]
        public ActionResult Create(int appId, int schemaId)
        {
            // Setup back url
            ViewBag.PreviousUrl = Url.Action("Index", "Licenses", new { appId, schemaId });

            // Setup model
            var viewModel = _licenseService.SetupBuildLicenseModel(appId, schemaId, LoggedInUser);

            // Setup default view name
            var viewName = "ConsumerCreate";

            // Set view name for provider 
            if (viewModel.IsProvider)
            {
                viewName = "ProviderCreate";
            }

            // Return view
            return View(viewName, viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("applications/{appId}/schemas/{schemaId}/licenses/create")]
        public ActionResult Create(int appId, int schemaId, BuildLicenseModel model)
        {
            // Check whether posted data has selected sections
            if (model == null || !model.Sections.Any())
            {
                throw new BaseException("Invalid data.");
            }

            // Check whether it's a request for license preview
            if (Request.Form["preview"] != null)
            {
                // Get file result
                var result = _licenseFiles.GetTemplatedLicenseForPreview(model.Sections, LoggedInUser.Organization.ID, schemaId, LoggedInUser);

                // Setup stream
                var stream = new MemoryStream(result.Content);

                // Return file
                return File(stream, result.MimeType, result.FileName);
            }

            // Setup default redirect url
            var redirectUrl = Url.Action("Index", "Licenses", new { appId, schemaId });

            // Check whether processing request for provider
            if (!model.IsProvider)
            {
                throw new BaseException("Only provider can create licenses");
            }

            _licenseService.CreateProviderTemplatedLicense(appId, schemaId, model, LoggedInUser);

            // Setup status message
            Toastr.Success("License for schema was successfully created.");

            // Return result
            return Redirect(redirectUrl);
        }

        [HttpPost]
        [Route("applications/{appId}/schemas/{schemaId}/licenses/upload")]
        public ActionResult UploadCustomLicense(int appId, int schemaId, HttpPostedFileBase file)
        {
            if (file == null)
            {
                throw new BaseException("File not found");
            }

            // Get stream with file
            var stream = new MemoryStream();
            file.InputStream.CopyTo(stream);

            // Save custom license to database
            _customLicenses.Add(appId, schemaId, stream, file.ContentType, file.FileName, LoggedInUser);

            // Redirect to list view
            return RedirectToAction("Index", new { appId, schemaId });
        }

        [HttpPost]
        [Route("applications/{appId}/schemas/{schemaId}/licenses/save-providers")]
        public ActionResult SaveProviders(int appId, int schemaId, List<DataProvider> providers)
        {
            // Save selected proivder custom licenses
            _licenseService.CreateConsumerCustomLicense(appId, schemaId, providers, LoggedInUser);

            // Redirect to list view
            return RedirectToAction("Index", new { appId, schemaId });
        }
        
        [Route("applications/{appId}/schemas/{schemaId}/licenses/Find")]
        public ActionResult FindSchemaProviders(int appId, int schemaId)
        {
            var model = _licenseComparer.GetSchemaProviders(schemaId, LoggedInUser);

            ViewBag.PreviousUrl = Url.Action("Create", "Licenses", new { appId, schemaId });

            return View("ConsumerMatches", model);
        }

        [Route("applications/{appId}/schemas/{schemaId}/licenses/{licenseId}/request-access")]
        public ActionResult RequestProviderAccess(int appId, int schemaId, int licenseId)
        {
            // Create tempalted license and save license matches
            _licenseService.CreateConsumerTemplatedLicense(appId, schemaId, licenseId, LoggedInUser);

            return Json(new { message = "Request has been sent for an approval" }, JsonRequestBehavior.AllowGet);
        }

    }
}