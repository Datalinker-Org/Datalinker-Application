using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using DataLinker.Services.Mappings;
using Rezare.CommandBuilder.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace DataLinker.Services.LicenseTemplates
{
    internal class LicenseTemplatesService : ILicenseTemplatesService
    {
        private IService<LicenseTemplate, int> _templates;
        private IService<LicenseSection, int> _sections;
        private IService<LicenseAgreement, int> _agreements;
        private IService<Organization, int> _organisations;
        private IService<DataSchema, int> _schemas;
        private IService<OrganizationLicense, int> _licenses;
        private IService<OrganizationLicenseClause, int> _orgClauses;
        private IService<LicenseClause, int> _clauses;
        private IService<LicenseClauseTemplate, int> _clauseTemplates;
        private IService<License, int> _genericLicenses;
        private IService<Application, int> _applications;
        private IService<ConsumerProviderRegistration, int> _consumerProviderRegistrations;

        private DateTime GetDate => DateTime.UtcNow;

        public LicenseTemplatesService(IService<LicenseTemplate, int> templates,
            IService<LicenseSection, int> sections,
            IService<LicenseAgreement, int> agreements,
            IService<Organization, int> organisations,
            IService<DataSchema, int> schemas,
            IService<OrganizationLicense, int> licenses,
            IService<OrganizationLicenseClause, int> licenseClauses,
            IService<LicenseClause, int> clauses,
            IService<License, int> genericLicenses,
            IService<Application, int> applications,
            IService<ConsumerProviderRegistration, int> consumerRegistrations,
            IService<LicenseClauseTemplate, int> clauseTemplates)
        {
            _templates = templates;
            _sections = sections;
            _agreements = agreements;
            _organisations = organisations;
            _schemas = schemas;
            _licenses = licenses;
            _orgClauses = licenseClauses;
            _clauses = clauses;
            _clauseTemplates = clauseTemplates;
            _genericLicenses = genericLicenses;
            _applications = applications;
            _consumerProviderRegistrations = consumerRegistrations;
        }


        public List<LicenseTemplateDetails> GetLicenseTemplates(bool includeRetracted, LoggedInUserDetails user)
        {
            // Define result
            var result = new List<LicenseTemplateDetails>();

            // Return error if user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Build list of models for displaying
            result = new List<LicenseTemplateDetails>();

            // Get all available templates
            var templates = _templates.All();
            if (!includeRetracted)
            {
                templates = templates.Where(i => i.Status != (int)TemplateStatus.Retracted).ToList();
            }

            foreach (var licenseTemplate in templates)
            {
                // Build model for template
                var item = licenseTemplate.ToModel();
                // Add model to list of models
                result.Add(item);
            }

            // Return result
            return result.OrderByDescending(i => i.CreatedAt).ToList();
        }

        public LicenseTemplateDetails GetLicenseModel(int id, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get license template
            var license = _templates.FirstOrDefault(i=>i.ID == id);

            // Build model for this template
            var model = license.ToModel();

            // Setup license content
            if (!string.IsNullOrEmpty(license.LicenseText))
            {
                var document = new XmlDocument();
                document.LoadXml(license.LicenseText);
                model.LicenseText = document.OuterXml;
            }

            return model;
        }

        public void SaveLicenseTemplate(LicenseTemplateDetails model, byte[] file, LoggedInUserDetails user)
        {
            // Return error if user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Check if this is update
            if (model.ID != null)
            {
                // Get license template
                var licenseTemplate = _templates.FirstOrDefault(i => i.ID == model.ID.Value);
                if (licenseTemplate.Status == (int)TemplateStatus.Retracted)
                {
                    throw new BaseException("Retracted license can not be updated.");
                }
                // Check whether template should be updated
                if (licenseTemplate.Status == (int)TemplateStatus.Draft)
                {
                    // Update license template logic
                    UpdateLicenseTemplate(model, file, licenseTemplate, user);
                    return;
                }
            }

            // Create license template, new or from active
            CreateLicenseTemplate(model, file, user);
        }

        public LicenseTemplateDetails GetEditModel(int id, LoggedInUserDetails user)
        {
            // Check whether user is admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get license template
            var license = _templates.FirstOrDefault(i=>i.ID == id);

            // Setup model
            var model = license.ToModel();

            // Setup text for license
            if (!string.IsNullOrEmpty(license.LicenseText))
            {
                var document = new XmlDocument();
                document.LoadXml(license.LicenseText);
                model.LicenseText = document.OuterXml;
            }

            return model;
        }

        public void PublishTemplate(int id, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get template
            var license = _templates.FirstOrDefault(i=>i.ID == id);

            // Check whether license is retracted
            if (license.Status == (int)PublishStatus.Retracted)
            {
                throw new BaseException("Only draft license can be published.");
            }

            // Check whether license is already active
            if (license.Status == (int)PublishStatus.Published)
            {
                throw new BaseException("License already published.");
            }

            // Get acitve license
            var publishedLicense = _templates.FirstOrDefault(i => i.Status == (int)TemplateStatus.Active);

            // Check whether active license present
            if (publishedLicense != null)
            {
                // Setup retract details for published license template
                publishedLicense.UpdatedAt = GetDate;
                publishedLicense.UpdatedBy = user.ID;
                publishedLicense.Status = (int)TemplateStatus.Retracted;

                // Save changes
                _templates.Update(publishedLicense);
            }

            // Setup publish details for template
            license.UpdatedAt = GetDate;
            license.UpdatedBy = user.ID;
            license.Status = (int)TemplateStatus.Active;

            // Save changes
            _templates.Update(license);
        }

        public void RetractLicenseTemplate(int id, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get license template
            var license = _templates.FirstOrDefault(i=>i.ID == id);

            // Check whether license is retracted
            if (license.Status == (int)PublishStatus.Retracted)
            {
                throw new BaseException("License alredy retracted.");
            }

            // Setup retraction details
            license.UpdatedAt = GetDate;
            license.UpdatedBy = user.ID;
            license.Status = (int)TemplateStatus.Retracted;

            // Save changes
            _templates.Update(license);
        }

        public CustomFileDetails GetFileDetails(int fileId, LoggedInUserDetails user)
        {
            // Check whether user is admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied");
            }

            // Get license template
            var licenseTemplate = _templates.FirstOrDefault(i=>i.ID == fileId);

            // Check whether template not exists
            if (licenseTemplate == null)
            {
                throw new BaseException("File not found");
            }

            // Get bytes for license content
            var bytes = Encoding.UTF8.GetBytes(licenseTemplate.LicenseText);

            // Setup file result
            var fileResult = new CustomFileDetails()
            {
                Content = bytes,
                FileName = licenseTemplate.Name + ".html",
                MimeType = "application/html"
            };
            return fileResult;
        }

        public CustomFileDetails GetReportDetails(LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get all valid license agreements
            var licenseAgreements = _agreements.Where(i => i.ExpiresAt == null);

            // Get all sections
            var licenseSections = _sections.All().ToList();

            // Get section titles
            var sectionTitles = GetHeaderForReport(licenseSections);

            // Append Header for report
            var reportHeader = "Date executed,Provider,Consumer,Schema" + sectionTitles;

            var fileContent = new StringBuilder();
            var reportBody = new StringBuilder();

            // Process each license agreement
            foreach (var licenseAgreement in licenseAgreements)
            {
                // Add line to report for license agreement
                reportBody.AppendLine(GetRecordForLicense(licenseAgreement, licenseSections));
            }

            // Add Header to file
            fileContent.AppendLine(reportHeader);

            // Add Body to file
            fileContent.Append(reportBody);

            // Setup file name
            var fileName = $"License_Agreements_{GetDate.ToString("yyyy MMMM dd")}.csv";

            // Setup file result
            var fileResult = new CustomFileDetails
            {
                Content = Encoding.UTF8.GetBytes(fileContent.ToString()),
                FileName = fileName,
                MimeType = "text/csv"
            };
            return fileResult;
        }

        private string GetHeaderForReport(List<LicenseSection> licenseSections)
        {
            var result = new StringBuilder();
            // Add title of each section
            foreach (var section in licenseSections)
            {
                // Add separator
                result.Append($",{section.Title}");
            }
            return result.ToString();
        }

        private string GetRecordForLicense(LicenseAgreement licenseAgreement, List<LicenseSection> licenseSections)
        {
            var record = new StringBuilder();

            var consumerRegistration = _consumerProviderRegistrations.GetById(licenseAgreement.ConsumerProviderRegistrationId);
            var providerLicense = _licenses.FirstOrDefault(i => i.ID == consumerRegistration.OrganizationLicenseID);
            var consumerApp = _applications.FirstOrDefault(i => i.ID == consumerRegistration.ConsumerApplicationID);
            var providerApp = _applications.FirstOrDefault(i => i.ID == providerLicense.ApplicationID);

            var providerOrg = _organisations.GetById(providerApp.OrganizationID);
            var consumerOrg = _organisations.GetById(consumerApp.OrganizationID);

            // Get schema
            var schema = _schemas.FirstOrDefault(i=>i.ID == providerLicense.DataSchemaID);
            // Add agreement details
            record.Append($"{licenseAgreement.CreatedAt},{providerOrg.Name},{consumerOrg.Name},{schema.Name}");
            // Get organization license clauses
            var orgLicenseClauses = _orgClauses.Where(i => i.OrganizationLicenseID == providerLicense.ID).ToList();
            // Get license clauses for each organization license clause
            var licenseClauses = orgLicenseClauses.Select(i => _clauses.FirstOrDefault(p=>p.ID == i.LicenseClauseID)).ToList();
            // Get count of used clauses
            var count = licenseSections.Count;
            // Process each section
            for (int i = 0; i < count; i++)
            {
                // Add separator
                record.Append(",");
                // Get license clause template for section
                var licenseClause = licenseClauses.FirstOrDefault(p => p.LicenseSectionID == licenseSections[i].ID);
                if (licenseClause != null)
                {
                    // Get clause template
                    var clauseTemplate = _clauseTemplates.FirstOrDefault(o=>o.LicenseClauseID == licenseClause.ID);
                    // Add short text to the record
                    record.Append($",{clauseTemplate.ShortText}");
                }
            }
            return record.ToString();
        }

        /// <summary>
        /// Update existing license templates from model
        /// </summary>
        /// <param name="model"></param>
        private void UpdateLicenseTemplate(LicenseTemplateDetails model, byte[] file, LicenseTemplate licenseTemplate, LoggedInUserDetails user)
        {
            if (licenseTemplate == null)
            {
                throw new BaseException("License template with such Id was not found");
            }

            // Return error if license is retracted
            if (licenseTemplate.Status == (int)TemplateStatus.Retracted)
            {
                throw new BaseException("Retracted license can't be processed");
            }

            // Process attached license file
            UpdateSectionsFromTemplateFile(file, user);

            // Setup license details
            licenseTemplate.Name = model.Name;
            licenseTemplate.LicenseText = model.LicenseText;
            licenseTemplate.Description = model.Description;
            licenseTemplate.UpdatedAt = GetDate;
            licenseTemplate.UpdatedBy = user.ID;
            licenseTemplate.Version += 1;
            // Save changes
            _templates.Update(licenseTemplate);
        }

        /// <summary>
        /// Create new license template from model
        /// </summary>
        /// <param name="model"></param>
        private void CreateLicenseTemplate(LicenseTemplateDetails model, byte[] file, LoggedInUserDetails user)
        {
            var licenseTemplate = new LicenseTemplate();

            UpdateSectionsFromTemplateFile(file, user);
            if (model.IsActive)
            {
                licenseTemplate.Version += 1;
            }
            else
            {
                licenseTemplate.Version = 1;
            }
            // Setup license details
            licenseTemplate.LicenseID = model.LicenseId;
            licenseTemplate.LicenseText = model.LicenseText;
            licenseTemplate.Status = (int)PublishStatus.Draft;
            licenseTemplate.Name = model.Name;
            licenseTemplate.Description = string.IsNullOrEmpty(model.Description) ? string.Empty : model.Description;
            licenseTemplate.UpdatedAt = GetDate;
            licenseTemplate.UpdatedBy = user.ID;
            licenseTemplate.CreatedAt = GetDate;
            licenseTemplate.CreatedBy = user.ID.Value;
            // Create new license if no licenses
            // ??? What is the purpose of generic licenses
            var globalLicense = _genericLicenses.All().FirstOrDefault();
            if (globalLicense == null)
            {
                var license = new License
                {
                    CreatedBy = user.ID.Value,
                    CreatedAt = GetDate,
                };
                _genericLicenses.Add(license);
                licenseTemplate.LicenseID = license.ID;
            }
            else
            {
                // We have only one published license
                licenseTemplate.LicenseID = globalLicense.ID;
            }
            // Add new license
            _templates.Add(licenseTemplate);
        }

        /// <summary>
        /// Get Section list. Save sections that not in db (save difference)
        /// </summary>
        /// <param name="stream">The source file</param>
        private void UpdateSectionsFromTemplateFile(byte[] file, LoggedInUserDetails user)
        {
            if(file.Length == 0)
            {
                return;
            }
            
            var document = new XmlDocument();
            var stream = new MemoryStream(file);
            document.Load(stream);
            var documentSections = document.GetElementsByTagName("section");
            // Get all sections
            var existingSections = _sections.All();

            // Get section titles
            var existingSectionTitles = existingSections.Select(p => p.Title).ToList();

            var sectionList = GetSectionsFromDocument(documentSections);

            // Get titles that are not present in database
            var diff = sectionList.Except(existingSectionTitles).ToList();

            // Save new sections
            foreach (var item in diff)
            {
                var section = new LicenseSection
                    {
                        Title = item,
                        CreatedAt = GetDate,
                        CreatedBy = user.ID.Value
                    };

                // Save section
                _sections.Add(section);
            }
        }
        
        private List<string> GetSectionsFromDocument(XmlNodeList documentSections)
        {
            // Define result
            var result = new List<string>();

            // Identify sections that needs to be saved
            foreach (XmlNode section in documentSections)
            {
                // Get section headers
                var isHeaderSection = section.Attributes["title"].Value.Contains("_header");
                if (!isHeaderSection)
                {
                    // Add not header sections
                    result.Add(section.Attributes["title"].Value);
                }
            }

            return result;
        }
    }
}