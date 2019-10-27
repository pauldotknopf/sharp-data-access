using System;
using System.Linq;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;
using SharpDataAccess.Data;

namespace SharpDataAccess.Migrations.Impl
{
    public class Migrator : IMigrator
    {
        private readonly IDataService _dataService;
        private readonly IMigrationsBuilder _migrationsBuilder;
        private readonly IDataAccessLogger _dataAccessLogger;

        public Migrator(IDataService dataService, IMigrationsBuilder migrationsBuilder, IDataAccessLogger dataAccessLogger)
        {
            _dataService = dataService;
            _migrationsBuilder = migrationsBuilder;
            _dataAccessLogger = dataAccessLogger;
        }
        
        public void Migrate()
        {
            using (var db = _dataService.OpenDbConnection())
            {
                db.CreateTable<Migration>();

                var installedMigrations = db.Select<Migration>();

                using (var transaction = db.OpenTransaction())
                {
                    _migrationsBuilder.BuildMigrations(migrations =>
                    {
                        if (migrations.Select(x => x.Version).Distinct().Count() != migrations.Count)
                        {
                            throw new Exception("Duplicate versions detected.");
                        }
                        
                        foreach (var migration in migrations)
                        {
                            if (installedMigrations.Any(x => x.Version == migration.Version))
                            {
                                // Already done!
                                continue;
                            }

                            _dataAccessLogger.Info($"Running migration {migration.GetType().Name}");
                            
                            migration.Run(db);

                            // Now that we ran the migration, let's add a record of it.
                            db.Insert(new Migration {Version = migration.Version, AppliedOn = DateTimeOffset.UtcNow});
                        }
                    });
                    
                    transaction.Commit();
                }
            }
        }

        public int? GetCurrentVersion()
        {
            using (var conScope = new ConScope(_dataService))
            {
                var db = conScope.Connection;
                
                db.CreateTableIfNotExists<Migration>();
                var migrations = db.Select<Migration>().ToList();
                if (migrations.Count == 0)
                {
                    return null;
                }
                return migrations.OrderByDescending(x => x.Version).First().Version;
            }
        }

        public class Migration
        {
            [AutoIncrement]
            public int Id { get; set; }
            
            public int Version { get; set; }
            
            public DateTimeOffset AppliedOn { get; set; }
        }
    }
}