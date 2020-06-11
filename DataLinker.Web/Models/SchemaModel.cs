using DataLinker.Services.Enums;
using DataLinker.Services.Schemas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DataLinker.Web.Models
{
    public class SchemaModel
    {
        public int DataSchemaID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public PublishStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }
    }
}