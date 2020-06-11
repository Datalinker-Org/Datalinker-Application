using System.Web.Mvc;

namespace DataLinker.Web.Helpers
{
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
#if DEBUG
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
#else
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "https://www.datalinker.org");
#endif
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "false");
            base.OnActionExecuting(filterContext);
        }
    }
}