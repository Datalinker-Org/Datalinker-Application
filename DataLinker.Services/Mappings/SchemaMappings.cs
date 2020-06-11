using DataLinker.Database.Models;
using DataLinker.Models;
using DataLinker.Models.Enums;

namespace DataLinker.Services.Mappings
{
    public static class SchemaMappings
    {
        public static SchemaModel ToModel(this DataSchema item)
        {
            var result = new SchemaModel
            {
                Name = item.Name,
                DataSchemaID = item.ID,
                PublicId = item.PublicID,
                Description = item.Description,
                Status = (TemplateStatus)item.Status,
                PublishedAt = item.PublishedAt
            };

            return result;
        }

        public static SchemaDetails ToDetails(this DataSchema schema)
        {
            var result = new SchemaDetails();
            result.public_id = schema.PublicID;
            result.name = schema.Name;
            result.description = schema.Description;
            result.is_aggregate = schema.IsAggregate;

            return result;
        }

        public static SoftwareStatementSchema ToStmtSchema(this DataSchema schema)
        {
            var result = new SoftwareStatementSchema();
            result.public_id = schema.PublicID;
            result.name = schema.Name;
            result.description = schema.Description;
            result.is_aggregate = schema.IsAggregate;

            return result;
        }
    }
}