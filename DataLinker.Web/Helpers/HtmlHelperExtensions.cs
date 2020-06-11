using DataLinker.Models.Enums;
using DataLinker.Web.Models.Users;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using DataLinker.Models;

namespace DataLinker.Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders a Bootstrap label (span with class label) with the PublishStatus.
        /// </summary>
        /// <param name="htmlHelper">Object to extend.</param>
        /// <param name="status">Publish status.</param>
        /// <param name="htmlAttributes">Optional additional HTML attributes.</param>
        /// <param name="isProvider">Is license of an provider application</param>
        /// <returns></returns>
        public static MvcHtmlString LicensePublishStatusLabel(this HtmlHelper htmlHelper, PublishStatus status,
            object htmlAttributes = null, bool isProvider=true)
        {
            var span = new TagBuilder("span");
            if (htmlAttributes != null)
            {
                span.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }

            span.AddCssClass("label");
            span.AddCssClass("status");
            switch (status)
            {
                case PublishStatus.Retracted:
                    span.AddCssClass("label-danger");
                    break;
                case PublishStatus.Draft:
                    span.AddCssClass("label-default");
                    break;
                case PublishStatus.PendingApproval:
                    span.AddCssClass("label-info");
                    break;
                case PublishStatus.ReadyToPublish:
                    span.AddCssClass("label-primary");
                    break;
                case PublishStatus.Published:
                    span.AddCssClass("label-success");
                    break;
                default:
                    span = new TagBuilder("span");
                    span.AddCssClass("glyphicon glyphicon-question-sign");
                    span.Attributes.Add("title", "No data agreement.");
                    return new MvcHtmlString(span.ToString(TagRenderMode.Normal));
            }
            
            if (isProvider)
            {
                span.SetInnerText(status.ToStringWithSpaces());
            }
            else
            {
                span.SetInnerText(GetConsumerStatus(status));
            }

            return new MvcHtmlString(span.ToString(TagRenderMode.Normal));
        }

        private static string GetConsumerStatus(PublishStatus status)
        { 
            switch (status)
            {
                case PublishStatus.Retracted:
                case PublishStatus.Draft:
                case PublishStatus.PendingApproval:
                    return status.ToStringWithSpaces();
                case PublishStatus.ReadyToPublish:
                    return "Pending Finalizing";
                case PublishStatus.Published:
                    return "Finalized";
                default:
                    return status.ToString();
            }
        }

        public static MvcHtmlString StatusLabelTemplate(this HtmlHelper htmlHelper, TemplateStatus status, object htmlAttributes = null)
        {
            var span = new TagBuilder("span");
            if (htmlAttributes != null)
            {
                span.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }
            span.AddCssClass("label");
            switch (status)
            {
                case TemplateStatus.Draft:
                    span.AddCssClass("label-info");
                    break;
                case TemplateStatus.Active:
                    span.AddCssClass("label-success");
                    break;
                case TemplateStatus.Retracted:
                    span.AddCssClass("label-danger");
                    break;
                default:
                    span.AddCssClass("label-default");
                    break;
            }

            span.GenerateId("Status");
            span.SetInnerText(status.ToString());

            return new MvcHtmlString(span.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString MatchClauseLabel(this HtmlHelper htmlHelper, bool isMatch, object htmlAttributes = null)
        {
            var span = new TagBuilder("span");
            if (htmlAttributes != null)
            {
                span.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }

            span.AddCssClass("label");
            span.AddCssClass(isMatch ? "label-success" : "label-danger");
            span.SetInnerText(isMatch ? "Match" : "Not Match");

            return new MvcHtmlString(span.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString CheckBoxForActivationStatus<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, bool>> expression, UserModel user, bool isSysAdmin, string url)
        {
            return user.IsActive || isSysAdmin ? htmlHelper.CheckBoxFor(expression, new {@class = "account-status", data_accountid = user.ID, data_url = url, data_isadmin = isSysAdmin, data_issinglelegal = user.IsSingleLegalOfficer}) : htmlHelper.CheckBoxFor(expression, new {@disabled = "true"});
        }
        
        // <summary>
        // Get the name of a static or instance property from a property access lambda.
        // </summary>
        // <typeparam name="T">Type of the property</typeparam>
        // <param name="propertyLambda">lambda expression of the form: '() => Class.Property' or '() => object.Property'</param>
        // <returns>The name of the property</returns>
        public static string GetPropertyName<T>(Expression<Func<T>> propertyLambda)
        {
            var me = propertyLambda.Body as MemberExpression;

            if (me == null)
            {
                throw new ArgumentException("You must pass a lambda of the form: '() => Class.Property' or '() => object.Property'");
            }

            return me.Member.Name;
        }

        /// <summary>
        /// Renders a Bootstrap label (span with class label) with the Service Type.
        /// </summary>
        /// <param name="htmlHelper">Object to extend.</param>
        /// <param name="isProvider">Flag indicating if this is a provider service.</param>
        /// <param name="htmlAttributes">Optional additional HTML attributes.</param>
        /// <returns></returns>
        public static MvcHtmlString ServiceTypeLabel(this HtmlHelper htmlHelper, bool isProvider, object htmlAttributes = null)
        {
            var span = new TagBuilder("span");
            if (htmlAttributes != null)
            {
                span.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }
            span.AddCssClass("label");

            span.SetInnerText(isProvider ? "Provider" : "Consumer");
            span.AddCssClass("label-info");

            return new MvcHtmlString(span.ToString(TagRenderMode.Normal));
        }

        /// <summary>
        /// Renders a Bootstrap label (span with class label) with the Service classification.
        /// </summary>
        /// <param name="htmlHelper">Object to extend.</param>
        /// <param name="isProvider">Flag indicating if this is a provider service.</param>
        /// <param name="htmlAttributes">Optional additional HTML attributes.</param>
        /// <returns></returns>
        public static MvcHtmlString ServiceClassificationLabel(this HtmlHelper htmlHelper, bool isIndustryGood, object htmlAttributes = null)
        {
            if (!isIndustryGood)
            {
                return new MvcHtmlString("");
            }

            var span = new TagBuilder("span");
            if (htmlAttributes != null)
            {
                span.MergeAttributes(new RouteValueDictionary(htmlAttributes));
            }
            span.AddCssClass("label");
            span.SetInnerText("Industry Good");
            span.AddCssClass("label-info");

            return new MvcHtmlString(span.ToString(TagRenderMode.Normal));
        }

        public static MvcHtmlString NavigationPanel(this HtmlHelper htmlHelper, LoggedInUserDetails user)
        {
            // Setup default value for navigation
            string html = string.Empty;

            // Navigation for unauthorized user
            if (user != null)
            {
                // Setup navigation for user
                html = GetNavigationForUser(htmlHelper, user.Organization.ID);
            }

            // Navigation for Admin
            if (user != null && user.IsSysAdmin)
            {
                html = GetNavigationForAdmin(htmlHelper);
            }

            return new MvcHtmlString(html);
        }

        /// <summary>
        /// Navbar sign in / sign out component used in _Layout.cshtml.
        /// </summary>
        /// <param name="htmlHelper">Object to extend.</param>
        /// <param name="user">The currently logged in user.</param>
        /// <returns></returns>
        public static MvcHtmlString SignInComponent(this HtmlHelper htmlHelper, LoggedInUserDetails user, UrlHelper Url)
        {
            string html;

            if (user == null)
            {
                html = string.Format(@"<ul class=""nav navbar-nav navbar-right""><li>{1}</li><li>{0}</li></ul>", htmlHelper.ActionLink("LOGIN", "Index", "Home"), htmlHelper.ActionLink("SIGNUP", "Create", "Account"));
            }
            else
            {
                var userHint = string.Empty;

                html = string.Format(@"
<ul class=""nav navbar-nav navbar-right"">
    <li class=""dropdown"">
        <a href=""#"" class=""dropdown-toggle"" data-toggle=""dropdown"" role=""button"" aria-haspopup=""true"" aria-expanded=""false"">
            {0}<span class=""caret""></span>
        </a>
        <ul class=""dropdown-menu"">
             <li>{1}</li>
             <li><a href=""#"" class=""account-edit-btn"" data-url=""{2}"">
                <small><span class=""glyphicon glyphicon-pencil""></span></small> Edit
                </a></li>
             <li><a href=""{3}"">
                <small><span class=""glyphicon glyphicon-off""></span></small> Sign Out
                </a>
             </li>
        </ul>
    </li>
</ul>", htmlHelper.Encode(user.Email), htmlHelper.Encode(userHint), Url.Action("Edit", "Account", new {controller = "Account", organizationId=user.Organization.ID, userId = user.ID}), Url.Action("SignOut", "Account"));
            }

            return new MvcHtmlString(html);
        }

        /// <summary>
        /// Converts new lines in blocks of text to double BR tags to give
        /// appearance of paragraphs.
        /// </summary>
        /// <param name="htmlHelper">Helper to extend.</param>
        /// <param name="text">Text to modify.</param>
        /// <returns>Text with new lines replaced.</returns>
        public static MvcHtmlString RenderLineBreaks(this HtmlHelper htmlHelper, string text)
        {
            return new MvcHtmlString(Regex.Replace(htmlHelper.Encode(text), "[\r\n]+", "<br/><br/>"));
        }

        public static MvcHtmlString RenderLongUrl(this HtmlHelper htmlHelper, string text)
        {
            return new MvcHtmlString(htmlHelper.Encode(text).Replace("/", "/&#8203;"));
        }

        private static string GetNavigationForUser(HtmlHelper htmlHelper,int organizationId)
        {
            return $@"<li>{htmlHelper.ActionLink("DASHBOARD", "Details", "Home",new { organizationId },null)}</li>
                            <li><a href=""https://www.datalinker.org.nz/about""> ABOUT </a></li>
                            <li class=""dropdown""><a href=""#"" class=""dropdown-toggle"" data-toggle=""dropdown"" role=""button"" aria-haspopup=""true"" aria-expanded=""false""> REPOSITORIES <span class=""caret""></span></a>
                                <ul class=""dropdown-menu"">
                                    <li><a href=""https://github.com/Datalinker-Org/Spatial"">Spatial Data</a></li>
                                    <li><a href=""https://github.com/Datalinker-Org/Pasture-Management"">Pasture Management</a></li>
                                    <li><a href=""https://github.com/Datalinker-Org/Farm-Data"">Farm Data</a></li>
                                    <li><a href=""https://github.com/Datalinker-Org/Benchmarks"">Benchmarks</a></li>
                                    <li><a href=""https://github.com/Datalinker-Org/Farm-Assurance"" >Farm Assurance</a></li>
                                    <li><a href=""https://github.com/Datalinker-Org/Animal-Data"" >Animal Data</a></li>
                                </ul>
                            </li>
                            <li><a href=""https://www.datalinker.org/contact""> CONTACT US </a></li>
                            ";
        }

        private static string GetNavigationForAdmin(HtmlHelper htmlHelper)
        {
            return string.Format(@"<li>{0}</li>
                                    <li>{1}</li>
                            <li class=""dropdown""><a href=""#"" class=""dropdown-toggle"" data-toggle=""dropdown"" role=""button"" aria-haspopup=""true"" aria-expanded=""false""> Templates <span class=""caret""></span></a>
                                <ul class=""dropdown-menu"">
                                    <li>{2}</li>
                                    <li>{3}</li>
                                </ul>
                            </li>
                            ", htmlHelper.ActionLink("Dashboard", "AdminDashboard", "Home"), htmlHelper.ActionLink("Services", "Index", "Applications"), htmlHelper.ActionLink("Licenses", "Index", "LicenseTemplates"), htmlHelper.ActionLink("Clauses", "Index", "LicenseClauses"));
        }

        public static MvcHtmlString ClauseForConsumer<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, BuildLicenseModel model, int i, int j, object htmlAttributes = null)
        {
            var templateType = (ClauseType) model.Sections[i].Clauses[j].Type;
            var htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            // Build checkbox for clause
            var checkBox = htmlHelper.CheckBoxFor(m => model.Sections[i].Clauses[j].IsSelectedByConsumer, htmlAttributeDictionary);

            // Get collection name to build element's id and name
            var clauseCollection = GetPropertyName(() => model.Sections[i].Clauses);
            switch (templateType)
            {
                case ClauseType.Text:
                {
                    var clauseText = htmlHelper.DisplayFor(p => model.Sections[i].Clauses[j].LegalText);
                    var res = $"{checkBox} {clauseText}";
                    return new MvcHtmlString(res);
                }

                case ClauseType.Input:
                {
                    // TODO refactor this
                    var isAttribution = model.Sections[i].Section.Title.ToLower() == "attribution";
                    var inputModel = model.Sections[i].Clauses[j];
                    var inputValue = GetPropertyName(() => inputModel.InputValue);

                    // Build input
                    var input = new TagBuilder("input");
                    var inputId = $"Sections[{i}].{clauseCollection}[{j}].{inputValue}";
                    input.MergeAttributes(new RouteValueDictionary(new {id = inputId, name = inputId, value = inputModel.InputValue}));
                    var textBox = new MvcHtmlString(input.ToString(TagRenderMode.Normal));
                    // Get clause text to display with input
                    var indexOfOpenBracket = model.Sections[i].Clauses[j].LegalText.IndexOf('{');
                    var clauseText = htmlHelper.DisplayFor(p => inputModel.LegalText);

                    // Remove brackets from text
                    var textWithInput = new string(clauseText.ToString().Where(p => p != '{' && p != '}').ToArray());

                    // Do not display attribution textbox for consumer
                    if (!isAttribution)
                    {
                        // Insert input on a brackets location
                        textWithInput = textWithInput.Insert(indexOfOpenBracket, $" {textBox} ");
                    }

                    return new MvcHtmlString($"{checkBox} {textWithInput}");
                }

                case ClauseType.InputAndDropDown:
                {
                    var inputDropDownModel = model.Sections[i].Clauses[j];
                    var inputValue = GetPropertyName(() => inputDropDownModel.InputValue);
                    var dropDownValue = GetPropertyName(() => inputDropDownModel.SelectedItem);

                    // Build input and drop down
                    var input = new TagBuilder("input");
                    var inputId = $"Sections[{i}].{clauseCollection}[{j}].{inputValue}";
                    var dropDownId = $"Sections[{i}].{clauseCollection}[{j}].{dropDownValue}";
                    input.MergeAttributes(new RouteValueDictionary(new {name = inputId, id = inputId, value = inputDropDownModel.InputValue }));
                    var textBox = new MvcHtmlString(input.ToString(TagRenderMode.Normal));

                        // Build DropDown
                        var dropDown = htmlHelper.DropDownList(dropDownId, inputDropDownModel.Source
                            .Select(item => new SelectListItem { Text = item.Text, Value = item.Value, Selected = item.IsSelected })
                            .ToList(), new { id = dropDownId, name = dropDownId });

                    // get location of first '{' - input, and second '{','}' - start and end of drop down items
                    var inputLocation = inputDropDownModel.LegalText.IndexOf('{');
                    var dropDownStart = inputDropDownModel.LegalText.LastIndexOf('{');
                    var dropDownEnd = inputDropDownModel.LegalText.LastIndexOf('}');

                    // Get clause text to display
                    var clauseText = htmlHelper.DisplayFor(p => inputDropDownModel.LegalText);

                    // remove dropdown list items from text
                    var textWithInputAndDropDown = clauseText.ToString().Remove(dropDownStart, dropDownEnd - dropDownStart);

                    // Remove brackets from text
                    textWithInputAndDropDown = new string(textWithInputAndDropDown.Where(p => p != '{' && p != '}').ToArray());

                    // Insert input element into input location
                    textWithInputAndDropDown = textWithInputAndDropDown.Insert(inputLocation, $" {textBox} ");

                    // Insert DropDown into drop down location(+ move as input inserted)
                    textWithInputAndDropDown = textWithInputAndDropDown.Insert(dropDownStart + textBox.ToString().Length, $" {dropDown} ");

                    return new MvcHtmlString($"{checkBox} {textWithInputAndDropDown}");
                }
                default:
                    throw new Exception("Unknown clause type.");
            }
        }

        public static MvcHtmlString ClauseForProvider<TModel, TValue>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TValue>> expression, BuildLicenseModel model, int i, int j, object htmlAttributes = null)
        {
            var templateType = (ClauseType) model.Sections[i].Clauses[j].Type;
            var isOptional = model.Sections[i].Section.Title.Contains("Optional");
            var htmlAttributeDictionary = HtmlHelper.AnonymousObjectToHtmlAttributes(htmlAttributes);

            // Build radio button for clause
            var radioBtn = htmlHelper.RadioButtonFor(m => model.Sections[i].SelectedClause, model.Sections[i].Clauses[j].ClauseTemplateId, htmlAttributeDictionary);
            // Display optional as checkbox
            if (isOptional)
            {
                //Disable required: htmlAttributes = new { @data_val = true };
                radioBtn = htmlHelper.CheckBoxFor(m => model.Sections[i].Clauses[j].IsSelectedByConsumer, htmlAttributeDictionary);
            }
            // Get collection name to build element's id and name
            var clauseCollection = GetPropertyName(() => model.Sections[i].Clauses);
            switch (templateType)
            {
                case ClauseType.Text:
                {
                    var clauseText = htmlHelper.DisplayFor(p => model.Sections[i].Clauses[j].LegalText);
                    var res = $"{radioBtn} {clauseText}";
                    return new MvcHtmlString(res);
                }

                case ClauseType.Input:
                {
                    var inputModel = model.Sections[i].Clauses[j];
                    var inputValue = GetPropertyName(() => inputModel.InputValue);

                    // Build input
                    var input = new TagBuilder("input");
                    var inputId = $"Sections[{i}].{clauseCollection}[{j}].{inputValue}";
                    input.MergeAttributes(new RouteValueDictionary(new {name = inputId, id = inputId}));
                    var textBox = new MvcHtmlString(input.ToString(TagRenderMode.Normal));

                    // Get clause text to display with input
                    var indexOfOpenBracket = model.Sections[i].Clauses[j].LegalText.IndexOf('{');
                    var clauseText = htmlHelper.DisplayFor(p => inputModel.LegalText);

                    // Remove brackets from text
                    var textWithInput = new string(clauseText.ToString().Where(p => p != '{' && p != '}').ToArray());

                    // Insert input on a brackets location
                    textWithInput = textWithInput.Insert(indexOfOpenBracket, $" {textBox} ");

                    return new MvcHtmlString($"{radioBtn} {textWithInput}");
                }

                case ClauseType.InputAndDropDown:
                {
                    var inputDropDownModel = model.Sections[i].Clauses[j];
                    var inputValue = GetPropertyName(() => inputDropDownModel.InputValue);
                    var dropDownValue = GetPropertyName(() => inputDropDownModel.SelectedItem);

                    // Build input
                    var input = new TagBuilder("input");
                    var inputId = $"Sections[{i}].{clauseCollection}[{j}].{inputValue}";
                    var dropDownId = $"Sections[{i}].{clauseCollection}[{j}].{dropDownValue}";
                    input.MergeAttributes(new RouteValueDictionary(new {name = inputId, id = inputId}));
                    var textBox = new MvcHtmlString(input.ToString(TagRenderMode.Normal));

                    // Build DropDown
                    var dropDown = htmlHelper.DropDownList(dropDownId, inputDropDownModel.Source
                        .Select(item => new SelectListItem { Text = item.Text, Value = item.Value, Selected = item.IsSelected })
                        .ToList(), new {id = dropDownId, name = dropDownId});

                    // get location of first '{' - input, and second '{','}' - start and end of drop down items
                    var inputLocation = inputDropDownModel.LegalText.IndexOf('{');
                    var dropDownStart = inputDropDownModel.LegalText.LastIndexOf('{');
                    var dropDownEnd = inputDropDownModel.LegalText.LastIndexOf('}');

                    // Get clause text to display
                    var clauseText = htmlHelper.DisplayFor(p => inputDropDownModel.LegalText);

                    // remove dropdown list items from text
                    var textWithInputAndDropDown = clauseText.ToString().Remove(dropDownStart, dropDownEnd - dropDownStart);

                    // Remove brackets from text
                    textWithInputAndDropDown = new string(textWithInputAndDropDown.Where(p => p != '{' && p != '}').ToArray());

                    // Insert input element into input location
                    textWithInputAndDropDown = textWithInputAndDropDown.Insert(inputLocation, $" {textBox} ");

                    // Insert DropDown into drop down location(+ move as input inserted)
                    textWithInputAndDropDown = textWithInputAndDropDown.Insert(dropDownStart + textBox.ToString().Length, $" {dropDown} ");

                    return new MvcHtmlString($"{radioBtn} {textWithInputAndDropDown}");
                }
                default:
                    throw new Exception("Unknown clause type.");
            }
        }
    }
}