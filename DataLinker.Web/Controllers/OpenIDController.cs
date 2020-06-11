using DataLinker.Services.Users;
using System.IO;
using System.Net;
using System.Web.Mvc;
using DataLinker.Services.Authorisation;

namespace DataLinker.Web.Controllers
{
    /// <summary>
    /// The purpose of this controller is to simply relay a .well-known URL request
    /// to the DataLinker UI. Querying .well-known URLs directly from JavaScript is
    /// unreliable because of cross-domain request restrictions, so this Controller
    /// handles the request and relays the response back to the web UI.
    /// </summary>
    public class OpenIDController : BaseController
    {
        public OpenIDController(IAuthorisationService auth) : base(auth) { }

        [Route("open-id/well-known")]
        public ActionResult Index(string url)
        {
            var httpRequest = WebRequest.CreateHttp(url);
            httpRequest.Accept = "application/json";

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                | SecurityProtocolType.Ssl3;

            var httpResponse = httpRequest.GetResponse();
            var config = string.Empty;
            using (var reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                config = reader.ReadToEnd();
            }

            return Content(config, "application/json");
        }
    }
}