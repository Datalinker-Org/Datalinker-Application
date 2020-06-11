using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;
using DataLinker.Services.Exceptions;
using Rezare.CommandBuilder.Services;
using System.Linq;

namespace DataLinker.Services.OrganizationLicenses
{
    internal class OrganizationLicenseClauseService : IOrganizationLicenseClauseService
    {
        private readonly IService<OrganizationLicenseClause, int> _service;
        private readonly IService<LicenseClauseTemplate, int> _clauseTemplates;

        public OrganizationLicenseClauseService(IService<OrganizationLicenseClause, int> organisationClauses,
            IService<LicenseClauseTemplate, int> clauseTemplates)
        {
            _service = organisationClauses;
            _clauseTemplates = clauseTemplates;
        }


        public bool IsClauseSelected(SectionsWithClauses section)
        {
            // In Optional section expected checkbox clause
            if (section.Section.Title.Contains("Optional"))
            {
                // Get optional clause
                var optionalClause = section.Clauses.First();
                // Setup value, if selected
                if (optionalClause.IsSelectedByConsumer)
                {
                    section.SelectedClause = section.Clauses.First().ClauseId;
                }
            }
            if (section.SelectedClause <= 0)
            {
                return false;
            }
            return true;
        }

        public string GetClauseData(ClauseModel selectedClause)
        {
            // Get template for clause
            var clauseTemplate = _clauseTemplates.FirstOrDefault(i => i.LicenseClauseID == selectedClause.ClauseId);

            switch ((ClauseType)selectedClause.Type)
            {
                case ClauseType.Text:
                    {
                        return selectedClause.LegalText;
                    }
                case ClauseType.Input:
                    {
                        var model = selectedClause;

                        if (string.IsNullOrEmpty(model.InputValue))
                        {
                            return clauseTemplate.LegalText;
                        }
                        // Identify position where text should be inserted
                        var indexOfOpenBracket = clauseTemplate.LegalText.IndexOf('{');

                        // Insert text into brakets
                        // Result will be like: charged at a rate of ${10}
                        var result = clauseTemplate.LegalText.Insert(indexOfOpenBracket + 1, model.InputValue);

                        // Return result
                        return result;
                    }
                case ClauseType.InputAndDropDown:
                    {
                        // Check whether input has value
                        if (string.IsNullOrEmpty(selectedClause.InputValue))
                        {
                            selectedClause.InputValue = "0";
                        }

                        // Legal Text example: Consumer of data will be charged at a rate of ${} each {day,week,month}.
                        var model = selectedClause;
                        var indexOfOpenBracket = clauseTemplate.LegalText.IndexOf('{');
                        var index2OfOpenBracket = clauseTemplate.LegalText.LastIndexOf('{');
                        var index2OfCloseBracket = clauseTemplate.LegalText.LastIndexOf('}');

                        // TODO: Make sure entered value validation does not allow to input '{' '}'
                        // remove dropdown list items from text
                        var legalText = clauseTemplate.LegalText.Remove(index2OfOpenBracket + 1,
                            index2OfCloseBracket - index2OfOpenBracket - 1);

                        // Update index after text modification
                        index2OfCloseBracket = legalText.LastIndexOf('}');

                        // Insert DropDown selected value into a brackets location
                        legalText = legalText.Insert(index2OfCloseBracket, model.SelectedItem);

                        // insert value of input and selected dropdown item to template 
                        // Result will be like: charged at a rate of ${10} each {day}
                        return legalText.Insert(indexOfOpenBracket + 1, model.InputValue);
                    }
                default:
                    throw new BaseException("Unknown clause type");
            }
        }
    }
}