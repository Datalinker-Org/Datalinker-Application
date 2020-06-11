using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataLinker.Models;
using DataLinker.Services.Applications;
using DataLinker.Services.Schemas;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;

namespace DataLinker.WebApi.Controllers
{
    public class SchemasController : BaseApiController
    {
        private readonly IDataSchemaService _schemaService;

        public SchemasController(IApplicationsService applicationService,
            IDataSchemaService schemaService) : base(applicationService)
        {
            _schemaService = schemaService;
        }

        /// <summary>
        ///     Gets all published data schemas
        /// </summary>
        /// <param name="isAggregateOnly">Returns aggregate schemas only if set to True</param>
        /// <returns>List of schemas</returns>
        [AcceptVerbs("GET")]
        [SwaggerGroupName(GroupNames.Consumer)]
        [SwaggerResponse(HttpStatusCode.OK, "Returns list of schemas.", typeof(IEnumerable<SchemaDetails>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, "Unable to process request.")]
        [Route("api/schemas")]
        [Route("schemas")]
        public HttpResponseMessage Get(bool isAggregateOnly = false)
        {
            try
            {
                Log.Info($"Get schemas [Begin]: return only aggregate - {isAggregateOnly}");
                var result = _schemaService.GetSchemas(isAggregateOnly);
                Log.Info($"Get schemas [End]: result - {JsonConvert.SerializeObject(result)}");
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                Log.Error(ex);
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}
