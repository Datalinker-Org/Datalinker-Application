using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace DataLinker.Models
{
    public class ClauseModel
    {
        [Display(Name = "Legal Text")]
        public string LegalText { get; set; }

        public bool IsSelectedByConsumer { get; set; }

        public int ClauseTemplateId { get; set; }

        public int ClauseId { get; set; }

        public int Type { get; set; }

        public int SectionId { get; set; }
        
        public string InputValue { get; set; }

        public string SelectedItem { get; set; }

        public List<SelectItem> Source { get; set; }

        public void SetupDropDownItems()
        {
            var result = GetItems();
            Source = new List<SelectItem>();

            // Init list items with retrieved data
            foreach (var item in result)
            {
                Source.Add(new SelectItem { Text = item, Value = item, IsSelected = item == SelectedItem });
            }
        }

        public List<string> GetItems()
        {
            var indexOfOpenBracket = LegalText.IndexOf('{');
            var index2OfOpenBracket = LegalText.LastIndexOf('{');
            var index2OfCloseBracket = LegalText.LastIndexOf('}');

            // remove dropdown list items from text
            var rawItems= LegalText.Substring(index2OfOpenBracket,
                index2OfCloseBracket - index2OfOpenBracket + 1);

            // Remove brackets
            var temp = new string(rawItems.ToCharArray().Where(i => i != '{' && i != '}').ToArray());

            // Split into items
            var result = temp.Split(',').ToList();

            // Return result
            return result;
        }
    }

    public class SelectItem
    {
        public string Text { get; set; }

        public string Value { get; set; }

        public bool IsSelected { get; set; }
    }
}