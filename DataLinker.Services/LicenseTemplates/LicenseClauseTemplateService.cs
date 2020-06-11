using System.Collections.Generic;
using System.Linq;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;
using System.Text;
using DataLinker.Services.Mappings;
using DataLinker.Services.Emails;
using Rezare.CommandBuilder.Services;
using DataLinker.Database.Models;
using DataLinker.Models;

namespace DataLinker.Services.LicenseTemplates
{
    public class LicenseClauseTemplateService : ILicenseClauseTemplateService
    {
        private IService<LicenseClauseTemplate, int> _clauseTemplates;
        private IService<LicenseClause, int> _clauses;
        private IService<LicenseSection, int> _sections;
        private IService<LicenseTemplate, int> _licenseTemplates;
        private INotificationService _notifications;
        private readonly Encoding _encoding = Encoding.Default;

        public LicenseClauseTemplateService(IService<LicenseClauseTemplate, int> clauseTemplates,
            IService<LicenseClause, int> clauses,
            IService<LicenseSection, int> sections,
            IService<LicenseTemplate, int> templates,
            INotificationService notifications
            )
        {
            _clauseTemplates = clauseTemplates;
            _clauses = clauses;
            _sections = sections;
            _licenseTemplates = templates;
            _notifications = notifications;
        }

        private DateTime GetDate => DateTime.UtcNow;

        public void CreateClauseTemplate(int sectionId, LicenseClauseTemplateModel model, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get clauses for section
            var clauses = _clauses.Where(p => p.LicenseSectionID == sectionId).ToList();

            // Setup available order position
            var lastOrderNumber = 0;
            if (clauses.Any())
            {
                lastOrderNumber = clauses.Max(p => p.OrderNumber);
            }

            // Setup license clause
            var licenseClause = new LicenseClause
            {
                LicenseSectionID = model.LicenseSectionId,
                CreatedAt = GetDate,
                CreatedBy = user.ID.Value,
                OrderNumber = lastOrderNumber + 1
            };

            // Save license clause
            _clauses.Add(licenseClause);

            // Setup clause template
            var clauseTemplate = new LicenseClauseTemplate
            {
                CreatedAt = GetDate,
                CreatedBy = user.ID.Value,
                Description = model.Description,
                LegalText = model.LegalText,
                LicenseClauseID = licenseClause.ID,
                ClauseType = (int)GetTypeForClause(model.LegalText),
                ShortText = model.ShortText,
                Status = (int)TemplateStatus.Draft,
                Version = 1
            };

            // Save clause template
            _clauseTemplates.Add(clauseTemplate);
        }

        public LicenseClauseTemplateModel GetClauseForEdit(int id, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get clause template
            var clauseTemplate = _clauseTemplates.FirstOrDefault(i => i.ID == id);

            // Check whether clause not found
            if (clauseTemplate == null)
            {
                throw new BaseException("Not found");
            }

            // Check whether clause is published
            if (clauseTemplate.Status == (int)TemplateStatus.Active)
            {
                throw new BaseException("You can't edit active clause template.");
            }

            // Get clause
            var clause = _clauses.FirstOrDefault(i => i.ID == clauseTemplate.LicenseClauseID);

            // Get section
            var section = _sections.FirstOrDefault(i => i.ID == clause.LicenseSectionID);

            // Setup model
            var model = clauseTemplate.ToModel();
            model.SectionName = section.Title;
            return model;
        }

        public void EditClauseTemplate(int id, LicenseClauseTemplateModel model, LoggedInUserDetails user)
        {
            // Check whether user is admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get clause template
            var clauseTemplate = _clauseTemplates.FirstOrDefault(i => i.ID == id);

            // Chekc whether clause is active
            if (clauseTemplate.Status==(int)TemplateStatus.Active)
            {
                throw new BaseException("You can't edit active clause template.");
            }

            // Check whether clause is not in draft status
            if (!model.IsDraft)
            {
                throw new BaseException("Unable to edit not draft clause");
            }

            // Setup update details
            clauseTemplate.UpdatedAt = GetDate;
            clauseTemplate.UpdatedBy = user.ID;
            var type = GetTypeForClause(model.LegalText);
            clauseTemplate.ClauseType = (int)type;
            clauseTemplate.LegalText = model.LegalText;
            clauseTemplate.ShortText = model.ShortText;
            clauseTemplate.Description = model.Description;

            // Save changes
            _clauseTemplates.Update(clauseTemplate);
        }

        public void CreateClausesForSection(int sectionId, byte[] file, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Check whether section does not exist
            var clauseSection = _sections.FirstOrDefault(i => i.ID == sectionId);
            if (clauseSection == null)
            {
                throw new BaseException($"Section not found {sectionId}");
            }
            
            // Process file
            var fileContent = _encoding.GetString(file);
            var stringReader = new StringReader(fileContent);
            var deserializeBuilder = new DeserializerBuilder();
            deserializeBuilder.WithNamingConvention(new CamelCaseNamingConvention());
            var deserializer = deserializeBuilder.Build();
            var sections = deserializer.Deserialize<List<ClauseFileModel>>(stringReader);
            if (sections != null)
            {
                foreach (var section in sections)
                {
                    foreach (var clause in section.clauses)
                    {
                        // Create clause template
                        CreateClauseTemplate(sectionId, clause,user.ID.Value);
                    }
                }
            }
        }

        public void PublishTemplate(int id, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get clause template
            var clauseTemplate = _clauseTemplates.FirstOrDefault(i => i.ID == id);

            // Check whether template is not in draft state
            if (clauseTemplate.Status != (int)TemplateStatus.Draft)
            {
                throw new BaseException("Only draft clause template can be published.");
            }

            // Setup publish details
            clauseTemplate.Status = (int)TemplateStatus.Active;

            // Save changes
            _clauseTemplates.Update(clauseTemplate);

            // Notify users about published clause template
            _notifications.User.NewClauseInBackground(clauseTemplate.ID);
        }

        public SectionsAndClausesModel GetSectionsWithClausesModel(LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Setup result model
            var result = new SectionsAndClausesModel
            {
                Sections = new List<SectionModel>()
            };

            // Get published license template
            var globalLicense = _licenseTemplates.FirstOrDefault(i => i.Status == (int)TemplateStatus.Active);
            if (globalLicense != null)
            {
                // Setup active template
                result.GlobalLicense = globalLicense.ToModel();
            }

            // Get all sections
            var allSections = _sections.All().ToList();

            // Setup clause templates
            foreach (var section in allSections)
            {
                var clauseTemplates = new List<LicenseClauseTemplateModel>();
                // Get clauses for section
                var clauses = _clauses.Where(p => p.LicenseSectionID == section.ID).ToList();
                foreach (var licenseClause in clauses)
                {
                    // Get clause templates for clause
                    var allTemplates = _clauseTemplates.Where(p => p.LicenseClauseID == licenseClause.ID);

                    // Do not display retracted templates
                    var templates = allTemplates.Where(i => i.Status != (int)TemplateStatus.Retracted).ToList();

                    // Setup clause template models
                    foreach (var template in templates)
                    {
                        // Setup clause model
                        var clauseTemplateModel = template.ToModel();

                        // Add to result
                        clauseTemplates.Add(clauseTemplateModel);
                    }
                }

                // Setup section model
                var sectionModel = new SectionModel
                {
                    ClauseTemplates = clauseTemplates.AsReadOnly(),
                    ID = section.ID,
                    Title = section.Title,
                    CreatedAt = section.CreatedAt,
                    UpdatedAt = section.UpdatedAt
                };

                // Add section model
                result.Sections.Add(sectionModel);
            }

            return result;
        }

        public LicenseClauseTemplateModel GetClauseModel(int sectionId, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get section
            var section = _sections.FirstOrDefault(i => i.ID == sectionId);
            if (section == null)
            {
                throw new BaseException("Not found.");
            }

            // Setup result model
            var result = new LicenseClauseTemplateModel
            {
                Status = TemplateStatus.Draft,
                LicenseSectionId = sectionId,
                SectionName = section.Title,
                Version = 1
            };

            // Return result
            return result;
        }

        public void RetractTemplate(int id, LoggedInUserDetails user)
        {
            // Check whether user is not admin
            if (!user.IsSysAdmin)
            {
                throw new BaseException("Access denied.");
            }

            // Get clause template
            var clauseTemplate = _clauseTemplates.FirstOrDefault(i => i.ID == id);

            // Check whether template already retracted
            if (clauseTemplate.Status != (int)TemplateStatus.Retracted)
            {
                // Setup retraction details
                clauseTemplate.Status = (int)TemplateStatus.Retracted;

                // Save changes
                _clauseTemplates.Update(clauseTemplate);
            }
        }

        private void CreateClauseTemplate(int sectionId, Clause clause, int userId)
        {
            // Setup clause
            var licenseClause = new LicenseClause
            {
                LicenseSectionID = sectionId,
                CreatedAt = GetDate,
                CreatedBy = userId,
                OrderNumber = clause.OrderNumber
            };

            // Save clause
            _clauses.Add(licenseClause);

            // Setup clause template
            var clauseTemplate = new LicenseClauseTemplate
            {
                Description = clause.Description,
                LegalText = clause.LegalText,
                ShortText = clause.ShortText,
                CreatedAt = GetDate,
                CreatedBy = userId,
                Status = (int)TemplateStatus.Draft,
                LicenseClauseID = licenseClause.ID,
                ClauseType = (int)GetTypeForClause(clause.LegalText),
                Version = 1
            };

            // Save clause template
            _clauseTemplates.Add(clauseTemplate);
        }

        private ClauseType GetTypeForClause(string legalText)
        {
            // TODO: Already implemented in other service??
            var array = legalText.ToCharArray();
            // Clause can content no more than 2 blocks of {} - if input and dropdown in using
            const int maxCountOfBracketsInLegalText = 2;
            const int countOfBracketsInBlock = 2; // Example of block: input - "{}", dropdown - "{item1,item2}"
            const char openBlock = '{';
            const int openBlockIndex = 0;
            const char closeBlock = '}';
            const int closeBlockIndex = 1;
            const int secondBlockIndex = 1;
            var indexSuite = new int[maxCountOfBracketsInLegalText][];
            for (var i = 0; i < indexSuite.Length; i++)
            {
                indexSuite[i] = new int[countOfBracketsInBlock];
            }

            // Calculate count of brackets in a text
            var countOpenBraсkets = 0;
            var countClosingBraсkets = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if (array[i] == openBlock)
                {
                    countOpenBraсkets++;
                    indexSuite[countOpenBraсkets - 1][openBlockIndex] = i;
                }
                if (array[i] == closeBlock)
                {
                    countClosingBraсkets++;
                    indexSuite[countClosingBraсkets - 1][closeBlockIndex] = i;
                }
            }

            if (countOpenBraсkets != countClosingBraсkets)
            {
                throw new BaseException("Invalid syntax near '{' or '}'.");
            }

            switch (countOpenBraсkets)
            {
                case 0:
                    return ClauseType.Text;
                case 1:
                    return ClauseType.Input;
                case 2:
                    {
                        var dropDownItems = legalText.Substring(indexSuite[secondBlockIndex][openBlockIndex],
                            indexSuite[secondBlockIndex][closeBlockIndex] - indexSuite[secondBlockIndex][openBlockIndex]);
                        dropDownItems = new string(dropDownItems.Where(p => p != openBlock && p != closeBlock).ToArray());
                        var result = dropDownItems.Split(',');
                        if (result.Length < 1)
                        {
                            throw new BaseException("Invalid syntax");
                        }

                        return ClauseType.InputAndDropDown;
                    }

                default:
                    throw new BaseException("Unknown clause type.");
            }
        }
    }
}