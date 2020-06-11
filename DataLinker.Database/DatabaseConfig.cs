using DataLinker.Database.Migrations;
using System.Data.Entity;

namespace DataLinker.Database
{
    public static class DatabaseConfig
    {
        public static void Initialize()
        {
            System.Data.Entity.Database.SetInitializer(new MigrateDatabaseToLatestVersion<DataLinkerContext, Configuration>());

            using (var db = new DataLinkerContext())
                db.Database.Initialize(true);
        }
    }
}
