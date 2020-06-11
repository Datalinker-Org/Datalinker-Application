using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using DataLinker.Services.Exceptions;
using DataLinker.Services.LicenseTemplates;
using DataLinker.Services.Authorisation;
using DataLinker.Models;

namespace DataLinker.Web.Controllers
{
    public class LicenseClausesController : BaseController
    {
        private readonly ILicenseClauseTemplateService _clauseTemplates;
        // TODO Webconfig
        private readonly string clauseExampleFileName = "Clause_example.yml";

        public LicenseClausesController(
            ILicenseClauseTemplateService clauseTemplateService, IAuthorisationService auth) : base(auth)
        {
            _clauseTemplates = clauseTemplateService;
        }

        private DateTime GetDate => DateTime.UtcNow;

        [Route("license-clauses")]
        public ActionResult Index()
        {
            // Setup model
            SectionsAndClausesModel result = _clauseTemplates.GetSectionsWithClausesModel(LoggedInUser);

            // Setup back Url
            ViewBag.PreviousUrl = Url.Action("Index", "Home");

            // Return result
            return View(result);
        }
        
        [Route("license-clauses/sections/{sectionId}/create-clause")]
        public ActionResult Create(int sectionId)
        {
            LicenseClauseTemplateModel model = _clauseTemplates.GetClauseModel(sectionId, LoggedInUser);

            // Setup back url
            ViewBag.PreviousUrl = Url.Action("Index", "LicenseClauses");

            // Return result
            return View(model);
        }
                
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("license-clauses/sections/{sectionId}/create-clause")]
        public ActionResult Create(int sectionId, LicenseClauseTemplateModel model)
        {
            // Create clause template
            _clauseTemplates.CreateClauseTemplate(sectionId, model, LoggedInUser);

            // Return result
            return RedirectToAction("Index");
        }
        
        [Route("license-clauses/{id}/edit")]
        public ActionResult Edit(int id)
        {
            // Get clause model
            LicenseClauseTemplateModel model =_clauseTemplates.GetClauseForEdit(id, LoggedInUser);

            // Setup back navigation
            ViewBag.PreviousUrl = Url.Action("Index", "LicenseClauses");
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("license-clauses/{id}/edit")]
        public ActionResult Edit(int id, LicenseClauseTemplateModel model)
        {
            // Edit clause template
            _clauseTemplates.EditClauseTemplate(id, model, LoggedInUser);

            return RedirectToAction("Index");
        }
        
        [HttpPost]
        [Route("license-clauses/sections/{sectionId}/upload-new-clause")]
        public ActionResult Upload(int sectionId)
        {
            // Get uploaded file
            var file = GetStreamWithFile();

            // todo: validate anti forgery
            _clauseTemplates.CreateClausesForSection(sectionId, file.ToArray(), LoggedInUser);

            // Setup redirect URL
            var redirectUrl = Url.Action("Index", "LicenseClauses");

            // Return result
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }
        
        [Route("license-clauses/download-template")]
        public FileResult GetClauseExample()
        {
            // Check whether user is not admin
            if(!LoggedInUser.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            var file = new FileStream($@"{AppDomain.CurrentDomain.BaseDirectory}/{clauseExampleFileName}", FileMode.Open);
            return File(file, MimeMapping.GetMimeMapping(clauseExampleFileName), clauseExampleFileName);
        }

        [Route("license-clauses/{id}/publish")]
        public ActionResult Publish(int id)
        {
            _clauseTemplates.PublishTemplate(id, LoggedInUser);

            // Setup redirect url
            var redirectUrl = Url.Action("Index", "LicenseClauses");
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }
        
        [Route("license-clauses/{id}/retract")]
        public ActionResult Retract(int id)
        {
            // Retract template
            _clauseTemplates.RetractTemplate(id, LoggedInUser);

            // Setup redirect url
            var redirectUrl = Url.Action("Index", "LicenseClauses");

            // Return result
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Gets the file content from request and puts it to stream
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