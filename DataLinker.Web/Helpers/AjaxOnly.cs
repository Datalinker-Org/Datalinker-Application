using System.Web.Mvc;
using DataLinker.Services.Exceptions;

namespace DataLinker.Web
{
    public class AjaxOnly: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Check whether request is not AJAX
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
            {
                throw new BaseException("Unable to process your request. Please try again later");
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
