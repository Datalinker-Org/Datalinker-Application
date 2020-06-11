using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataLinker.Models;
using DataLinker.Services.Exceptions;
using DataLinker.Services.SoftwareStatements;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using DataLinker.Services.Applications;

namespace DataLinker.WebApi.Controllers
{
    [SwaggerGroupName(GroupNames.Provider)]
    public class LicenseAgreementsController : BaseApiController
    {
        private readonly ISoftwareStatementService _softwareStatementService;

        public LicenseAgreementsController(IApplicationsService apps,
            ISoftwareStatementService softwareStatementService)
            : base(apps)
        {
            _softwareStatementService = softwareStatementService;
        }

        /// <summary>
        ///     Saves details about approved schemas during the registration request
        /// </summary>
        /// <returns>List of schemas</returns>
        [AcceptVerbs("POST")]
        [SwaggerResponse(HttpStatusCode.OK, "New data agreement recorded")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Unable to process request.")]
        [Route("api/licenses")]
        [Route("licenses")]
        public HttpResponseMessage CreateNew([FromBody] LicenseDetails licenseDetails)
        {
            try
            {
                Log.Info($"Create license agreement [Begin]: licenseDetails - {JsonConvert.SerializeObject(licenseDetails)}");
                _softwareStatementService.CreateLicenseAgreement(licenseDetails,LoggedInApplication);

                Log.Info("Create license agreement [End]");
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (BaseApiException ex)
            {
                Log.Info(ex.Message);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}