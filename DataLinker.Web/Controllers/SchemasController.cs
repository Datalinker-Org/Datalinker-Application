using System;
using System.IO;
using System.Web.Mvc;
using DataLinker.Services.Configuration;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Schemas;
using DataLinker.Web.Models.Schemas;
using PagedList;
using DataLinker.Services.Authorisation;
using DataLinker.Database.Models;

namespace DataLinker.Web.Controllers
{
    public class SchemasController : BaseController
    {
        private readonly IDataSchemaService _dataSchemaService;
        private readonly IConfigurationService _config;

        public SchemasController(IDataSchemaService dataSchemaService, IAuthorisationService auth,
            IConfigurationService configurationService)
            : base(auth)
        {
            _dataSchemaService = dataSchemaService;
            _config = configurationService;
        }

        public DateTime GetDate => DateTime.UtcNow;

        [Route("data-schemas")]
        public ActionResult Index(int? page, bool includeRetracted = false)
        {
            // Get schemas 
            var schemas = _dataSchemaService.GetSchemaModels(includeRetracted, LoggedInUser);

            // Setup back url
            ViewBag.PreviousUrl = Url.Action("Index", "Home");
            ViewBag.IncludeRetracted = includeRetracted;

            // Get page size from config
            var pageSize = _config.ManageSchemasPageSize;

            // Setup page number
            var pageNumber = page ?? 1;

            // Setup paged list
            var list = schemas.ToPagedList(pageNumber, pageSize);

            // Return view
            return View(list);
        }
        
        [Route("data-schemas/create")]
        public ActionResult Create()
        {
            // Check whether user has access
            if (!LoggedInUser.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Setup back url
            ViewBag.PreviousUrl = Url.Action("Index", "Schemas");

            // Return result
            return View(new SchemaModel {Status = TemplateStatus.Draft});
        }

        [HttpPost]
        [Route("data-schemas/create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(SchemaModel model)
        {
            // Check whether file present
            if (model.UploadFile == null)
            {
                Toastr.Error("You should upload a schema");
                return View(model);
            }

            // Setup stream 
            var stream = new MemoryStream();
            model.UploadFile.InputStream.CopyTo(stream);
            
            // Setup model
            var newSchema = new DataLinker.Models.SchemaModel
            {
                Description = model.Description,
                PublicId = model.PublicId,
                IsAggregate = model.IsAggregate,
                IsIndustryGood = model.IsIndustryGood,
                Name = model.Name,
                Status = TemplateStatus.Draft,
                Version = 1
            };

            // Create new schema
            _dataSchemaService.Create(newSchema, stream.ToArray(), model.UploadFile.FileName, LoggedInUser);

            // Setup status message
            Toastr.Success("Schema was successfully created");

            // Return result
            return RedirectToAction("Index");
        }

        [Route("data-schemas/validate-name")]
        public JsonResult IsSchemaNotExists(string name, string InitialName)
        {
            var result = _dataSchemaService.IsSchemaNameNotExists(name, InitialName);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Route("data-schemas/validate-public-identifier")]
        public JsonResult IsSchemaIdNotExists(string publicid, string initialId)
        {
            var result = _dataSchemaService.IsSchemaIdNotExists(publicid, initialId);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        
        [Route("data-schemas/{id}/edit")]
        public ActionResult Edit(int id)
        {
            // Get model
            var model = _dataSchemaService.GetModel(id, LoggedInUser);

            // Setup model
            var data = new SchemaModel
            {
                Description = model.Description,
                IsAggregate = model.IsAggregate,
                IsIndustryGood = model.IsIndustryGood,
                Name = model.Name,
                Status = model.Status,
                PublicId = model.PublicId,
                SchemaFileId = model.SchemaFileId
            };

            ViewBag.PreviousUrl = Url.Action("Index", "Schemas");
            return View(data);
        }

        [HttpPost]
        [Route("data-schemas/{id}/edit")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, SchemaModel model)
        {
            MemoryStream stream = null;
            var fileName = string.Empty;

            // Check whether file needs to be updated
            if (model.UploadFile != null)
            {
                // Setup file stream
                stream = new MemoryStream();
                fileName = model.UploadFile.FileName;
                model.UploadFile.InputStream.CopyTo(stream);
            }

            // Setup internal model
            var data = new DataLinker.Models.SchemaModel
            {
                DataSchemaID = id,
                Description = model.Description,
                IsAggregate = model.IsAggregate,
                IsIndustryGood = model.IsIndustryGood,
                Name = model.Name,
                PublicId = model.PublicId
            };

            // Update schema
            _dataSchemaService.Update(data, stream, fileName, LoggedInUser);

            // Setup status message
            Toastr.Success("Schema was successfully updated");

            // Return result
            return RedirectToAction("Index");
        }

        [Route("data-schemas/usage-this-month")]
        public FileResult GenerateReport()
        {
            // Get file result
            var result = _dataSchemaService.GetReport(LoggedInUser);

            // Return file
            return File(result.Content, result.MimeType, result.FileName);
        }

        [Route("data-schemas/{id}/publish")]
        public ActionResult PublishSchema(int id)
        {
            // Publish schema
            DataSchema dataSchema = _dataSchemaService.Publish(id, LoggedInUser);

            // Setup redirect url
            var redirectUrl = Url.Action("Index");

            // Setup status message
            Toastr.Success($"Schema {dataSchema.Name} was successfully published.");

            // Return response
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }
        
        [Route("data-schemas/{id}/retract")]
        public ActionResult RetractSchema(int id)
        {
            // Retract schema
            _dataSchemaService.Retract(id, LoggedInUser);

            // Setup redirect url
            var redirectUrl = Url.Action("Index");

            // Return result
            return Json(new { Url = redirectUrl }, JsonRequestBehavior.AllowGet);
        }
        
        [Route("data-schemas/{fileId}/download")]
        public FileResult Download(int fileId)
        {
            // Get file details
            var result =_dataSchemaService.GetFileDetails(fileId);

            // Return file result
            return File(result.Content, result.MimeType, result.FileName);
        }
    }
}