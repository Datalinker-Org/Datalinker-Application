using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using DataLinker.Models;
using DataLinker.Services.Authorisation;
using DataLinker.Services.Exceptions;
using DataLinker.Web.Helpers;
using DataLinker.Web.Models;
using log4net;

namespace DataLinker.Web.Controllers
{
    [AllowCrossSiteJson]
    [Authorize]
    [OutputCache(NoStore = true, Location = OutputCacheLocation.None)]
    public class BaseController : Controller
    {
        public static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Toastr _toastr;
        private readonly IAuthorisationService _auth;

        public BaseController(IAuthorisationService authrosation)
        {
            _auth = authrosation;
            _toastr = new Toastr(this);
        }
        
        public LoggedInUserDetails LoggedInUser { get; set; }

        /// <summary>
        ///     Helper object to display Toastr notifications when the page load completes.
        /// </summary>
        public Toastr Toastr
        {
            get { return _toastr; }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (User is ClaimsPrincipal)
            {
                var claims = (User as ClaimsPrincipal).Claims.ToList();
                // Get details about authorized user
                LoggedInUser = _auth.GetUserDetails(claims);
            }

            ViewData["LoggedInUser"] = LoggedInUser;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            var exception = filterContext.Exception;
            filterContext.ExceptionHandled = true;
            if (exception.GetType() == typeof (BaseException))
            {
                if (Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    filterContext.Result = new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new {msg=exception.Message}
                    };
                }
                else
                {
                    filterContext.Result = View("Error", new ErrorModel {Message = exception.Message});
                }
            }
            else
            {
                Log.Error(exception);
                filterContext.Result = View("Error", new ErrorModel { Message = "Oops looks like there was an error." });
            }
        }
    }
}