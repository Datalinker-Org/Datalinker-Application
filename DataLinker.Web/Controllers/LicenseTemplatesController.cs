using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using DataLinker.Services.Configuration;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseTemplates;
using DataLinker.Web.Models.Licenses;
using PagedList;
using DataLinker.Models;
using Ganss.XSS;
using System.Text;
using DataLinker.Services.Authorisation;

namespace DataLinker.Web.Controllers
{
    public class LicenseTemplatesController : BaseController
    {
        private readonly ILicenseTemplatesService _templates;
        private readonly IConfigurationService _configService;
        private readonly Encoding _encoding = Encoding.Default;

        public LicenseTemplatesController(ILicenseTemplatesService licenseTemplatesService,
            IAuthorisationService auth,
            IConfigurationService configService) : base(auth)
        {
            _templates = licenseTemplatesService;
            _configService = configService;
        }

        private DateTime GetDate => DateTime.UtcNow;
        
        [Route("license-templates")]
        public ActionResult Index(int? page, bool includeRetracted = false)
        {
            var templates = _templates.GetLicenseTemplates(includeRetracted, LoggedInUser);

            // Setup model
            var model = new LicenseTemplatesModel
            {
                IncludeRetracted = includeRetracted,
                IsActivePresent = templates.Any(i => i.Status == TemplateStatus.Active)
            };

            // Set flags for view
            ViewBag.PreviousUrl = Url.Action("Index", "Home");
            // Page settings
            var pageSize = _configService.ManageLicenseTemplatesPageSize;
            var pageNumber = page ?? 1;
            model.Templates = templates.ToPagedList(pageNumber, pageSize);
            return View(model);
        }
        
        [Route("license-templates/{id}/details")]
        public ActionResult Details(int id)
        {
            LicenseTemplateDetails model = _templates.GetLicenseModel(id, LoggedInUser);

            // Set Back URL
            ViewBag.PreviousUrl = Url.Action("Index", "LicenseTemplates");
            return View(model);
        }
        
        [Route("license-templates/create")]
        public ActionResult Create()
        {
            // Check whether user is not admin
            if (!LoggedInUser.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Setup model
            var licenseTemplate = new LicenseTemplateDetails
            {
                Status = TemplateStatus.Draft
            };

            ViewBag.PreviousUrl = Url.Action("Index", "LicenseTemplates");
            return View(licenseTemplate);
        }

        [Route("license-templates/create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(LicenseTemplateDetails model)
        {
            var file = GetStreamWithFile();
            var fileBytes = file.ToArray();
            // Process attached license file
            var licenseText = _encoding.GetString(fileBytes);
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedAttributes.Add("id");
            model.LicenseText = sanitizer.Sanitize(licenseText);

            // Save license template
            _templates.SaveLicenseTemplate(model, fileBytes, LoggedInUser);

            // Return result
            return RedirectToAction("Index");
        }
        
        [Route("license-templates/{id}/edit")]
        public ActionResult Edit(int id)
        {
            // Setup edit model
            LicenseTemplateDetails model = _templates.GetEditModel(id, LoggedInUser);

            ViewBag.PreviousUrl = Url.Action("Index", "LicenseTemplates");
            return View(model);
        }

        [Route("license-templates/{id}/publish")]
        public ActionResult Publish(int id)
        {
            // Publish template
            _templates.PublishTemplate(id, LoggedInUser);

            // Setup redirect url
            var redirectUrl = Url.Action("Index", "LicenseTemplates");

            // Return result
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }

        [Route("license-templates/{id}/retract")]
        public ActionResult Retract(int id)
        {
            // Retract template
            _templates.RetractLicenseTemplate(id, LoggedInUser);

            // Setup redirect url
            var redirectUrl = Url.Action("Index", "LicenseTemplates");

            // Return result
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }
        
        [Route("license-templates/{fileId}/download")]
        public FileResult Download(int fileId)
        {
            // Setup file details
            CustomFileDetails fileResult = _templates.GetFileDetails(fileId, LoggedInUser);

            // Return result
            return File(fileResult.Content, fileResult.MimeType, fileResult.FileName);
        }
        
        [Route("license-templates/generate-report")]
        public FileResult GenerateReport()
        {
            // Setup report details
            CustomFileDetails fileResult = _templates.GetReportDetails(LoggedInUser);

            // Return File
            return File(fileResult.Content, fileResult.MimeType, fileResult.FileName);
        }
        
        /// <summary>
        /// Gets the file content from request and puts it to stream
        /// </summary>
        /// <returns>Returns stream with content of file</returns>
        private MemoryStream GetStreamWithFile()
        {
            const int expectedIndexOfFile = 0;
            var stream = new MemoryStream();
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[expectedIndexOfFile];
                if (file != null && file.ContentLength > 0)
                {
                    file.InputStream.CopyTo(stream);
                }
            }
            else
            {
                throw new BaseException("File not found.");
            }

            return stream;
        }
    }
}