using DataLinker.Database.Models;
using DataLinker.Models;
using System.Collections.Generic;
using System.Xml;

namespace DataLinker.Services.LicenseContent
{
    public interface ILicenseContentBuilder
    {
        string GetFooterText(int status, string urlToDataLinker);
        XmlDocument GetLicenseContent(int organizationLicenseId);
        
        void InsertContentIntoNodes(XmlNodeList nodesList, string identifierValue, string content,
            string nodeIdentifier = "title");
        
        void InsertAgreementDetails(XmlDocument document, int agreementId, string linkToSchema, string urlToDataLinker);
        
        void InsertLicenseDetails(XmlDocument document, string urlToSchema, string urlToDataLinker, int organizationId, bool isProvider);

        void InsertLicenseDetails(XmlDocument document, string urlToSchema, string urlToDataLinker, int providerOrganizationId, int consumerOrganizationId);

        XmlDocument GetDocument(List<SectionsWithClauses> model, int schemaId, int organizationId, LicenseTemplate licenseTemplate, string urlToDataLinker, string urlToDownloadSchema);
    }
}