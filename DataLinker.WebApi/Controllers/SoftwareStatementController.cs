using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataLinker.Models;
using DataLinker.Services.SoftwareStatements;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using DataLinker.Services.Applications;

namespace DataLinker.WebApi.Controllers
{
    public class SoftwareStatementController : BaseApiController
    {
        private readonly ISoftwareStatementService _softwareStatementService;

        public SoftwareStatementController(ISoftwareStatementService softwareStatementService,
            IApplicationsService apps)
            : base(apps)
        {
            _softwareStatementService = softwareStatementService;
        }

        /// <summary>
        ///     Gets software statement
        /// </summary>
        /// <returns></returns>
        [AcceptVerbs("GET")]
        [SwaggerGroupName(GroupNames.Consumer)]
        [SwaggerResponse(HttpStatusCode.OK, "Returns software statement.", typeof(string))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Unable to process request.")]
        [Route("api/softwarestatement")]
        [Route("softwarestatement")]
        public HttpResponseMessage Get()
        {
            try
            {
                Log.Info("Get statement [Begin]");
                // Get software statement
                var validStatement = _softwareStatementService.Get(LoggedInApplication.ID, LoggedInApplication, LoggedInApplication.Organization.ID);
                Log.Info($"Get statement [End]: result - {validStatement}");
                return Request.CreateResponse(HttpStatusCode.OK, validStatement);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        ///     Validates software statement
        /// </summary>
        /// <param name="softwareStmt"></param>
        /// <param name="scope"></param>
        /// <returns></returns>
        [AcceptVerbs("POST")]
        [SwaggerGroupName(GroupNames.Provider)]
        [SwaggerResponse(HttpStatusCode.OK, "Software statement was successfully validated.",
            typeof(StatementValidationResult))]
        [SwaggerResponse(HttpStatusCode.BadRequest,
            "Validation of software statement completed with errors. Check error details in response.",
            typeof(StatementValidationResult))]
        [Route("api/softwarestatement/validate")]
        [Route("softwarestatement/validate")]
        public HttpResponseMessage Validate([FromBody] string softwareStmt, string scope)
        {
            try
            {
                Log.Info("Validate statement [Begin]");
                Log.Info($"Validate statement [scope]: {scope}");
                Log.Info($"Validate statement [stmt]: {softwareStmt}");
                StatementValidationResult result = _softwareStatementService.GetValidationResult(softwareStmt, scope, LoggedInApplication);

                Log.Info($"Validate statement [End]: result - {JsonConvert.SerializeObject(result)}");
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}