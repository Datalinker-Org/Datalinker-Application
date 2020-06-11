using DataLinker.Models.Enums;

namespace DataLinker.Models
{
    public class ClauseValue
    {
        public ClauseType Type { get; set; }

        public string ListItem { get; set; }

        public double? Number { get; set; }

        public ClauseValue(string orgClauseData, ClauseType type)
        {
            // TODO: Move to service
            // Setup values that will be used for comparison
            Type = type;
            switch (Type)
            {
                case ClauseType.Text:
                    break;
                case ClauseType.Input:
                    // Identify where text is located
                    var indexOfOpenBracket = orgClauseData.IndexOf('{');
                    var indexOfCloseBracket = orgClauseData.IndexOf('}');

                    // Get text located in brakets
                    // Result will be 10 for text: charged at a rate of ${10}
                    var result = orgClauseData.Substring(indexOfOpenBracket + 1, indexOfCloseBracket - 1 - indexOfOpenBracket);

                    // Try to parse number
                    double parsedValue = 0;
                    var isNumber = double.TryParse(result, out parsedValue);
                    if (isNumber)
                    {
                        // Setup number
                        Number = parsedValue;
                    }

                    break;
                case ClauseType.InputAndDropDown:
                    // Identify where text is located
                    indexOfOpenBracket = orgClauseData.IndexOf('{');
                    indexOfCloseBracket = orgClauseData.IndexOf('}');

                    // Get text located in brakets
                    // Result will be 10 for text: charged at a rate of ${10} per {request}
                    result = orgClauseData.Substring(indexOfOpenBracket + 1, indexOfCloseBracket - 1 - indexOfOpenBracket);

                    // Try to parse number
                    parsedValue = 0;
                    isNumber = double.TryParse(result, out parsedValue);
                    if (isNumber)
                    {
                        // Setup number
                        Number = parsedValue;
                    }
                    // Identify where selected item is located
                    indexOfOpenBracket = orgClauseData.LastIndexOf('{');
                    indexOfCloseBracket = orgClauseData.LastIndexOf('}');

                    // Get text located in brakets
                    // Result will be 10 for text: charged at a rate of ${10} per {request}
                    result = orgClauseData.Substring(indexOfOpenBracket + 1, indexOfCloseBracket - 1 - indexOfOpenBracket);
                    ListItem = result;
                    break;
                default:
                    throw new System.Exception("Unknown clause type");
            }
        }
    }
}