using DataLinker.Database.Models;
using DataLinker.Models;
using System.Collections.Generic;
using System.IO;

namespace DataLinker.Services.Schemas
{
    public interface IDataSchemaService
    {
        List<SchemaModel> GetSchemaModels(bool includeRetracted, LoggedInUserDetails user);

        void Create(SchemaModel model, byte[] fileContent, string fileName, LoggedInUserDetails user);

        SchemaModel GetModel(int schemaId, LoggedInUserDetails user);

        void Update(SchemaModel model, MemoryStream stream, string fileName, LoggedInUserDetails user);

        CustomFileDetails GetReport(LoggedInUserDetails user);

        DataSchema Publish(int dataSchemaId, LoggedInUserDetails user);

        void Retract(int dataSchemaId, LoggedInUserDetails user);

        CustomFileDetails GetFileDetails(int fileId);

        bool IsSchemaIdNotExists(string publicid, string initialId);

        bool IsSchemaNameNotExists(string name, string InitialName);

        IEnumerable<SchemaDetails> GetSchemas(bool isAggregateOnly);
    }
}
