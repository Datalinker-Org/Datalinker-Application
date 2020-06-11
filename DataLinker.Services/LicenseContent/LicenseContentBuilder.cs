using System.Collections.Generic;
using System.Linq;
using System.Xml;
using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.OrganizationLicenses;
using Rezare.CommandBuilder.Services;

namespace DataLinker.Services.LicenseContent
{
    public class LicenseContentBuilder: ILicenseContentBuilder
    {
        private readonly IService<LicenseClauseTemplate, int> _clauseTemplates;
        private readonly IOrganizationLicenseClauseService _licenseClauseService;
        private readonly IService<OrganizationLicense, int> _licenseService;
        private readonly IService<LicenseTemplate, int> _licenseTemplateService;
        private readonly IService<LicenseAgreement, int> _agreementService;
        private readonly IService<Organization, int> _organizationService;
        private readonly IService<User, int> _userService;
        private readonly IService<SchemaFile, int> _schemaFileService;
        private readonly IService<OrganizationLicenseClause, int> _licenseClauses;
        private readonly IService<LicenseClause, int> _genericClauses;
        private readonly IService<LicenseSection, int> _sections;
        private readonly IService<ConsumerProviderRegistration, int> _consumerRegistrations;
        private readonly IService<Application, int> _applicationService;

        public LicenseContentBuilder(IService<OrganizationLicense, int> licenseService,
            IOrganizationLicenseClauseService licenseClauseService,
            IService<LicenseTemplate, int> licenseTemplateService,
            IService<LicenseAgreement, int> agreementService,
            IService<Organization, int> organizationService,
            IService<SchemaFile, int> schemaFile,
            IService<OrganizationLicenseClause, int> organisationClauses,
            IService<LicenseClause, int> genericClauses,
            IService<LicenseSection, int> sections,
            IService<LicenseClauseTemplate, int> clauses,
            IService<ConsumerProviderRegistration, int> consumerRegistrations,
            IService<Application, int> applications,
            IService<User, int> userService)
        {
            _licenseService = licenseService;
            _licenseClauseService = licenseClauseService;
            _licenseTemplateService = licenseTemplateService;
            _agreementService = agreementService;
            _organizationService = organizationService;
            _userService = userService;
            _schemaFileService = schemaFile;
            _licenseClauses = organisationClauses;
            _genericClauses = genericClauses;
            _sections = sections;
            _clauseTemplates = clauses;
            _consumerRegistrations = consumerRegistrations;
            _applicationService = applications;
        }

        public string GetFooterText(int status,string urlToDataLinker)
        {
            if (status >= (int)PublishStatus.ReadyToPublish)
            {
                return $"<p><mark>This document records licence terms approved by an authorised signatory. These terms may be shared by DataLinker to facilitate the execution of licence agreements for data sharing with other parties. {urlToDataLinker}</mark></p>";
            }
            return $"<p><mark>DRAFT – This document represents proposed licence terms only and has not been approved by an authorised signatory. {urlToDataLinker}</mark></p>";
        } 

        public XmlDocument GetLicenseContent(int organizationLicenseId)
        {
            // Get License
            var license = _licenseService.FirstOrDefault(i=>i.ID == organizationLicenseId);
            // Get all organization license clauses
            var licenseClauses = _licenseClauses.Where(clause => clause.OrganizationLicenseID == license.ID).ToList();
            // Get license clauses for organization license clauses
            var clauses = licenseClauses.Select(i => _genericClauses.FirstOrDefault(p=>p.ID == i.LicenseClauseID)).ToList();
            // Get all sections for global license
            var sections = _sections.All().ToList();
            // Get license template
            var licenseTemplate = _licenseTemplateService.FirstOrDefault(i=>i.ID == license.LicenseTemplateID.Value);
            if (licenseTemplate == null)
            {
                throw new BaseException("Global license not found.");
            }
            var document = GetFormattedDocument(licenseTemplate, sections, licenseClauses, clauses);
            return document;
        }

        public XmlDocument GetDocument(List<SectionsWithClauses> model, int schemaId, int organizationId, LicenseTemplate licenseTemplate, string urlToDataLinker, string urlToDownloadSchema)
        {
            var document = new XmlDocument();
            document.LoadXml(licenseTemplate.LicenseText);
            var docSections = document.GetElementsByTagName(ClauseTagName);
            foreach (var section in model)
            {
                // Skip not selected section
                if (!_licenseClauseService.IsClauseSelected(section)) continue;

                var selectedClause = section.Clauses.First(p => p.ClauseTemplateId == section.SelectedClause);
                var clauseTemplate = _clauseTemplates.FirstOrDefault(i=>i.ID == selectedClause.ClauseTemplateId);

                switch ((ClauseType)selectedClause.Type)
                {
                    case ClauseType.Text:
                        {
                            InsertContentIntoNodes(docSections, section.Section.Title,
                                clauseTemplate.LegalText);
                            break;
                        }

                    case ClauseType.Input:
                        {
                            var input = selectedClause;
                            var indexOfOpenBracket = clauseTemplate.LegalText.IndexOf('{');
                            var temp = new string(clauseTemplate.LegalText.Where(p => p != '{' && p != '}').ToArray());
                            temp = temp.Insert(indexOfOpenBracket, input.InputValue ?? string.Empty);
                            InsertContentIntoNodes(docSections, section.Section.Title,
                                temp);
                            break;
                        }

                    case ClauseType.InputAndDropDown:
                        {
                            var inputAndDropDown = selectedClause;
                            var indexOfOpenBracket = clauseTemplate.LegalText.IndexOf('{');
                            var index2OfOpenBracket = clauseTemplate.LegalText.LastIndexOf('{');
                            var index2OfCloseBracket = clauseTemplate.LegalText.LastIndexOf('}');
                            // remove dropdown list items from text
                            var temp = clauseTemplate.LegalText.Remove(index2OfOpenBracket,
                                index2OfCloseBracket - index2OfOpenBracket + 1);
                            // Insert DropDown selected value into a brackets location
                            temp = temp.Insert(index2OfOpenBracket, inputAndDropDown.SelectedItem);
                            // Remove all brackets
                            temp = new string(temp.Where(p => p != '{' && p != '}').ToArray());
                            temp = temp.Insert(indexOfOpenBracket, inputAndDropDown.InputValue);
                            InsertContentIntoNodes(docSections, section.Section.Title,
                                temp);
                            break;
                        }
                    default:
                        throw new BaseException("Unknown clause type");
                }
            }
            // Get schema file
            var schemaFile = _schemaFileService.FirstOrDefault(i => i.DataSchemaID == schemaId);
            // Setup url to schema
            InsertLicenseDetails(document, urlToDownloadSchema, urlToDataLinker, organizationId, isProvider: true);

            // Return result
            return document;
        }

        public void InsertContentIntoNodes(XmlNodeList nodesList, string identifierValue, string content, string nodeIdentifier = "title")
        {
            foreach (XmlNode node in nodesList)
            {
                var isNotExists = node.Attributes[nodeIdentifier] != null;
                // Set clause text
                if (isNotExists && node.Attributes[nodeIdentifier].Value == identifierValue)
                {
                    if (string.IsNullOrEmpty(node.InnerText))
                    {
                        node.InnerText = content;
                    }
                    else
                    {
                        // Multiple consumer choices
                        var doc = node.OwnerDocument;
                        XmlElement separatorElem = doc.CreateElement("p");
                        XmlElement textElem = doc.CreateElement("p");
                        var separator = doc.CreateTextNode("OR");
                        var text = doc.CreateTextNode(content);
                        // Append separator as separate node
                        separatorElem.AppendChild(separator);
                        node.AppendChild(separatorElem);
                        // Append clause text after separator
                        textElem.AppendChild(text);
                        node.AppendChild(textElem);
                    }
                }
                // Set clause header
                if (isNotExists && node.Attributes[nodeIdentifier].Value == $"{identifierValue}_header")
                {
                    node.InnerText = identifierValue;
                }
            }
        }
        
        public void InsertAgreementDetails(XmlDocument document, int agreementId, string linkToSchema, string urlToDataLinker)
        {
            var agreement = _agreementService.FirstOrDefault(i=>i.ID == agreementId);
            var collectedDetails = new List<PartyDetails>();
            //
            collectedDetails.AddRange(GetGeneralAgreementDetails(agreement, linkToSchema));
            collectedDetails.Add(GetDataLinkerDetails(urlToDataLinker));

            var consumerRegistration = _consumerRegistrations.GetById(agreement.ConsumerProviderRegistrationId);
            var providerLicense = _licenseService.FirstOrDefault(i => i.ID == consumerRegistration.OrganizationLicenseID);
            var consumerApp = _applicationService.FirstOrDefault(i => i.ID == consumerRegistration.ConsumerApplicationID);
            var providerApp = _applicationService.FirstOrDefault(i => i.ID == providerLicense.ApplicationID);
            // Consumer details
            collectedDetails.AddRange(GetConsumerOrganizationDetails(consumerApp.OrganizationID));
            collectedDetails.AddRange(GetConsumerLegalOfficerDetails(consumerRegistration.ApprovedBy.Value));
            // Provider details
            collectedDetails.AddRange(GetProviderOrganizationDetails(providerApp.OrganizationID));
            collectedDetails.AddRange(GetProviderLegalOfficerDetails(consumerRegistration.ProviderApprovedBy.Value));
            // Insert collected details
            var nodes = document.GetElementsByTagName(DetailsTagName);
            foreach (var details in collectedDetails)
            {
                InsertContentIntoNodes(
                    nodesList: nodes,
                    identifierValue: details.NodeKey,
                    content: details.NodeValue,
                    nodeIdentifier: DetailsIdentifier);
            }
            // NOTE:Insert Fee (value + separate text) details not implemented yet
        }
        
        public void InsertLicenseDetails(XmlDocument document, string urlToSchema, string urlToDataLinker, int organizationId, bool isProvider)
        {
            // Setup DataLinker details
            var dataToInsert = new List<PartyDetails> { GetDataLinkerDetails(urlToDataLinker) };

            // Setup schema details
            dataToInsert.Add(GetSchemaDetails(urlToSchema));
            if (isProvider)
            {
                // Add provider details
                dataToInsert.AddRange(GetProviderOrganizationDetails(organizationId));
            }
            else
            {
                // Add consumer details
                dataToInsert.AddRange(GetConsumerOrganizationDetails(organizationId));
            }

            // Insert data
            var nodes = document.GetElementsByTagName(DetailsTagName);
            foreach (var details in dataToInsert)
            {
                InsertContentIntoNodes(
                    nodesList: nodes,
                    identifierValue: details.NodeKey,
                    content: details.NodeValue,
                    nodeIdentifier: DetailsIdentifier);
            }
        }

        public void InsertLicenseDetails(XmlDocument document, string urlToSchema, string urlToDataLinker, int providerOrganizationId, int consumerOrganizationId)
        {
            // Setup DataLinker details
            var dataToInsert = new List<PartyDetails> { GetDataLinkerDetails(urlToDataLinker) };

            // Setup schema details
            dataToInsert.Add(GetSchemaDetails(urlToSchema));
            
            // Add provider details
            dataToInsert.AddRange(GetProviderOrganizationDetails(providerOrganizationId));
            
            // Add consumer details
            dataToInsert.AddRange(GetConsumerOrganizationDetails(consumerOrganizationId));
            

            // Insert data
            var nodes = document.GetElementsByTagName(DetailsTagName);
            foreach (var details in dataToInsert)
            {
                InsertContentIntoNodes(
                    nodesList: nodes,
                    identifierValue: details.NodeKey,
                    content: details.NodeValue,
                    nodeIdentifier: DetailsIdentifier);
            }
        }

        private XmlDocument GetFormattedDocument(LicenseTemplate licenseTemplate, IList<LicenseSection> sections, IList<OrganizationLicenseClause> licenseClauses, IList<LicenseClause> clauses)
        {
            var document = new XmlDocument();
            document.LoadXml(licenseTemplate.LicenseText);
            var docSections = document.GetElementsByTagName(ClauseTagName);
            // Process clauses for each section
            foreach (var section in sections)
            {
                // Get clauses for this section
                var sectionClauses = clauses.Where(i => i.LicenseSectionID == section.ID).ToList();
                // Check if provider has clauses for section
                var licenseClausesForSection = new List<OrganizationLicenseClause>();
                foreach (var clause in sectionClauses)
                {
                    licenseClausesForSection.AddRange(licenseClauses.Where(o => o.LicenseClauseID == clause.ID));
                }
                // Skip if section was not selected
                if (!licenseClausesForSection.Any())
                {
                    continue;
                }
                foreach (var licenseClause in licenseClausesForSection)
                {
                    AddClauseToSection(docSections, section, licenseClause);
                }
            }
            return document;
        }

        private void AddClauseToSection(XmlNodeList docSections, LicenseSection section,
            OrganizationLicenseClause licenseClause)
        {
            // Get published clause template
            var publishedTemplate = _clauseTemplates.Where(i => i.LicenseClauseID == licenseClause.LicenseClauseID).FirstOrDefault(i => i.Status == (int)TemplateStatus.Active);

            // Get retracted clause template
            var retractedTemplate =
                _clauseTemplates.Where(i => i.LicenseClauseID == licenseClause.LicenseClauseID).FirstOrDefault(i => i.Status == (int)TemplateStatus.Retracted);

            // Return error if both not found
            if (publishedTemplate == null && retractedTemplate == null)
            {
                throw new BaseException("No published templates for this clause.");
            }
            // If published template was not found, so it was retracted - use it
            publishedTemplate = publishedTemplate ?? retractedTemplate;
            // Process clauses for section
            switch ((ClauseType)publishedTemplate.ClauseType)
            {
                case ClauseType.Text:
                case ClauseType.Input:
                case ClauseType.InputAndDropDown:
                    {
                        var temp = new string(licenseClause.ClauseData.Where(p => p != '{' && p != '}').ToArray());
                        InsertContentIntoNodes(docSections, section.Title, temp);
                        break;
                    }
                default:
                    throw new BaseException("Unknown clause type");
            }
        }
        
        private List<PartyDetails> GetProviderOrganizationDetails(int providerOrganizationId)
        {
            // todo: config, license key mapping
            var providerNameKey = "ProviderOrgName";
            var organization = _organizationService.FirstOrDefault(i=>i.ID == providerOrganizationId);
            return new List<PartyDetails>
            {
                new PartyDetails {NodeKey = providerNameKey, NodeValue = organization.Name}
            };
        }

        private List<PartyDetails> GetConsumerOrganizationDetails(int consumerOrganizationId)
        {
            // todo: config, license key mapping
            var providerNameKey = "ConsumerOrgName";
            var organization = _organizationService.FirstOrDefault(i=>i.ID == consumerOrganizationId);
            return new List<PartyDetails>
            {
                new PartyDetails {NodeKey = providerNameKey, NodeValue = organization.Name}
            };
        }

        private List<PartyDetails> GetProviderLegalOfficerDetails(int providerLegalOfficerId)
        {
            var providerLegalOfficerNameKey = "ProviderLegalOfficerName";
            var providerLegalOfficerEmailKey = "ProviderLegalOfficerEmail";
            var providerLegalOfficer = _userService.GetById(providerLegalOfficerId);
            return new List<PartyDetails>
            {
                new PartyDetails {NodeKey = providerLegalOfficerNameKey, NodeValue = providerLegalOfficer.Name},
                new PartyDetails {NodeKey = providerLegalOfficerEmailKey, NodeValue = providerLegalOfficer.Email}
            };
        }

        private List<PartyDetails> GetConsumerLegalOfficerDetails(int consumerLegalOfficerId)
        {
            var providerLegalOfficerNameKey = "ConsumerLegalOfficerName";
            var providerLegalOfficerEmailKey = "ConsumerLegalOfficerEmail";
            var consumerLegalOfficer = _userService.GetById(consumerLegalOfficerId);
            return new List<PartyDetails>
            {
                new PartyDetails {NodeKey = providerLegalOfficerNameKey, NodeValue = consumerLegalOfficer.Name},
                new PartyDetails {NodeKey = providerLegalOfficerEmailKey, NodeValue = consumerLegalOfficer.Email}
            };
        }

        private List<PartyDetails> GetGeneralAgreementDetails(LicenseAgreement agreement, string linkToSchema)
        {
            return new List<PartyDetails>
            {
                GetDateDetails(agreement),
                GetSchemaDetails(linkToSchema)
            };
        }

        private PartyDetails GetDateDetails(LicenseAgreement agreement)
        {
            var dateKey = "DateOfAgreement";
            return new PartyDetails {NodeKey = dateKey, NodeValue = agreement.CreatedAt.ToShortDateString()};
        }

        private PartyDetails GetSchemaDetails(string linkToSchema)
        {
            var schemaUrl = "DownloadSchemaUrl";
            return new PartyDetails {NodeKey = schemaUrl, NodeValue = linkToSchema};
        }

        private PartyDetails GetDataLinkerDetails(string urlToDataLinker)
        {
            var schemaUrl = "DataLinkerHost";
            return new PartyDetails { NodeKey = schemaUrl, NodeValue = urlToDataLinker };
        }

        private const string DetailsTagName = "span";
        private const string ClauseTagName = "section";
        private const string DetailsIdentifier = "id";

        private class PartyDetails
        {
            public string NodeKey { get; set; }
            public string NodeValue { get; set; }
        }
    }
}