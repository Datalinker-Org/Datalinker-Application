using System.Collections.Generic;
using DataLinker.Models;

namespace DataLinker.Services.ConsumerRequests
{
    public interface IConsumerRequestService
    {
        void ApproveRequest(int applicationId, int id, LoggedInUserDetails user);

        void DeclineRequest(int applicationId, int id, LoggedInUserDetails user);

        List<ConsumerRequestModel> GetConsumerRequestModels(int applicationId, LoggedInUserDetails user);

        int GetNotProcessedRequests(int applicationId, LoggedInUserDetails user);
    }
}