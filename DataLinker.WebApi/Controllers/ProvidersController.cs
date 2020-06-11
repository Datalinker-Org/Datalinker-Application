using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataLinker.Models;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using DataLinker.Services.Applications;

namespace DataLinker.WebApi.Controllers
{
    public class ProvidersController : BaseApiController
    {
        private readonly IApplicationsService _appService;

        public ProvidersController(IApplicationsService apps) : base(apps)
        {
            _appService = apps;
        }

        /// <summary>
        ///     Gets provider details based on finalized data agreement.
        /// </summary>
        /// <param name="schemas">Array of schema identifiers (e.g ["urn:nz:pri:dl:animal.traits"] )</param>
        /// <returns>Provider details</returns>
        [AcceptVerbs("POST")]
        [SwaggerGroupName(GroupNames.Consumer)]
        [SwaggerResponse(HttpStatusCode.OK, "Returns list of providers with endpoints.",
            typeof (IEnumerable<appDetails>))]
        [SwaggerResponse(HttpStatusCode.NotFound, "Providers were not found")]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Unable to process request.")]
        [Route("api/providers/schemas")]
        [Route("providers/schemas")]
        public HttpResponseMessage Get([FromBody] string[] schemas)
        {
            try
            {
                Log.Info($"Get providers [Begin]: schemas - {string.Join(" ", schemas)}");
                List<appDetails> resultModel = _appService.GetProviders(schemas, LoggedInApplication);
                Log.Info($"Get providers [End]: result - {JsonConvert.SerializeObject(resultModel)}");
                if (!resultModel.Any())
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                return Request.CreateResponse(HttpStatusCode.OK, resultModel);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}