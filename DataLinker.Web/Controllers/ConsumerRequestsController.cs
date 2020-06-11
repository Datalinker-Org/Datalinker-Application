using System.Collections.Generic;
using System.Web.Mvc;
using DataLinker.Services.Configuration;
using DataLinker.Models;
using PagedList;
using DataLinker.Services.ConsumerRequests;
using DataLinker.Services.Authorisation;

namespace DataLinker.Web.Controllers
{
    public class ConsumerRequestsController : BaseController
    {
        private readonly IConsumerRequestService _requests;
        private readonly IConfigurationService _configService;

        public ConsumerRequestsController(IAuthorisationService auth,
            IConsumerRequestService requests,
            IConfigurationService configService) : base(auth)
        {
            _requests = requests;
            _configService = configService;
        }
        
        [Route("applications/{applicationId}/consumer-requests")]
        public ActionResult Index(int applicationId, int? page)
        {
            List<ConsumerRequestModel> result =_requests.GetConsumerRequestModels(applicationId, LoggedInUser);
            var pageSize = _configService.ManageUsersPageSize;
            var pageNumber = page ?? 1;
            ViewBag.PreviousUrl = Url.Action("Details", "Applications", new { id = applicationId });
            return View(result.ToPagedList(pageNumber, pageSize));
        }

        [Route("applications/{applicationId}/consumer-requests/{id}/approve")]
        public ActionResult Approve(int applicationId, int id)
        {
            // Approve request
            _requests.ApproveRequest(applicationId, id, LoggedInUser);

            // Return result
            return new JsonResult();
        }

        [Route("applications/{applicationId}/consumer-requests/{id}/decline")]
        public ActionResult Decline(int applicationId, int id)
        {
            // Decline request
            _requests.DeclineRequest(applicationId, id, LoggedInUser);

            // Return result
            return new JsonResult();
        }

        [Route("applications/{applicationId}/pending-consumer-requests")]
        public ActionResult GetBadgeData(int applicationId)
        {
            // Get count of requests
            int result = _requests.GetNotProcessedRequests(applicationId, LoggedInUser);

            // Return result
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}