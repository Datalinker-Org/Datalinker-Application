using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using DataLinker.Models;
using DataLinker.Services.Applications;
using DataLinker.Services.Exceptions;
using log4net;
//using Rezare.AuditLog;

namespace DataLinker.WebApi.Controllers
{

    public class BaseApiController : ApiController
    {
        private readonly IApplicationsService _applicationsService;
        //public readonly IAuditLogger AuditLog = AuditLogManager.GetAuditLogger();
        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private DateTime GetDate => DateTime.UtcNow;

        public BaseApiController(IApplicationsService applicationsService)
        {
            _applicationsService = applicationsService;
        }

        /// <summary>
        ///     Gets the currently logged in application, or null is no application
        ///     is logged in.
        /// </summary>
        public LoggedInApplication LoggedInApplication { get; set; }

        protected override void Initialize(HttpControllerContext controllerContext)
        {
            var requestUrl = string.Empty;
            try
            {
                base.Initialize(controllerContext);
                requestUrl = controllerContext.Request?.RequestUri?.AbsolutePath;
                
                // Get referer
                var providedUri = Request.Headers.Referrer;

                if (providedUri == null)
                {
                    throw new BaseException("Referrer required");
                }

                Log.Info($"Starting request for {providedUri}");
                // Get app token
                var authKey = controllerContext.Request.Headers.Authorization;

                if (authKey == null)
                {
                    throw new BaseException("Authorizaton token required");
                }

                Log.Info("API key found");
                var apiKey = HttpUtility.UrlDecode(authKey.ToString());
                LoggedInApplication = _applicationsService.GetLoggedInApp(providedUri.ToString(), apiKey);
            }
            catch (BaseException ex)
            {
                Log.Info($"[{requestUrl}] {ex.Message}");
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent(ex.Message)
                });
            }
            catch (Exception ex)
            {
                Log.Error($"[{requestUrl}] {ex}");
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Unauthorized,
                    Content = new StringContent("Unexpected error")
                });
            }
        }
    }
}