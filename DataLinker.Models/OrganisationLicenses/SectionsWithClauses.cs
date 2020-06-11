using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataLinker.Models
{
    public class SectionsWithClauses
    {
        public  bool IsForProvider { get; set; }

        public int ApplicationId { get; set; }

        public LicenseSectionModel Section { get; set; }

        [Required(ErrorMessage = "You should select at least one option")]
        public int SelectedClause { get; set; }

        public IList<ClauseModel> Clauses { get; set; }
    }
}