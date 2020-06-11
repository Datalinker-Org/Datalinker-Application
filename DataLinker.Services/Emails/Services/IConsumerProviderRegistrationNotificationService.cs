namespace DataLinker.Services.Emails.Services
{
    public interface IConsumerProviderRegistrationNotificatonService
    {   
        void ConsumerLegalApproval(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId);
        void ConsumerLegalApprovalRequestInBackground(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId);
        void ProviderLegalApproval(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId);
        void ProviderLegalApprovalRequestInBackground(int userId, string linkToConfirmScreen, string linkToSchema, string linkToDataLinker, string schemaName, int consumerProviderRegistrationId);
        void ProviderLegalApproveRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker);
        void ProviderLegalApprove(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker);
        void ConsumerRegistrationCompletionRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker);
        void ConsumerRegistrationCompletion(int userId, int consumerProviderRegistrationId, string schemaName, string linkToSchema, string linkToDataLinker);
        void ConsumerRegistrationDeclineByProviderLegalRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName);
        void ConsumerRegistrationDeclineByProviderLegal(int userId, int consumerProviderRegistrationId, string schemaName);
        void ConsumerRegistrationDeclineByConsumerLegalRequestInBackground(int userId, int consumerProviderRegistrationId, string schemaName);
        void ConsumerRegistrationDeclineByConsumerLegal(int userId, int consumerProviderRegistrationId, string schemaName);
    }
}
