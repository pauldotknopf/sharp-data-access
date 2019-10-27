using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpDataAccess.Migrations.Impl
{
    public class MigrationsBuilder : IMigrationsBuilder
    {
        private readonly IMigrationTypesProvider _migrationTypesProvider;
        private readonly IMigrationBuilder _migrationBuilder;

        public MigrationsBuilder(IMigrationTypesProvider migrationTypesProvider,
            IMigrationBuilder migrationBuilder)
        {
            _migrationTypesProvider = migrationTypesProvider;
            _migrationBuilder = migrationBuilder;
        }
        
        public void BuildMigrations(Action<IList<IMigration>> action)
        {
            var migrations = new List<IMigration>();
            foreach (var migrationType in _migrationTypesProvider.GetMigrationTypes())
            {
                migrations.Add(_migrationBuilder.Create(migrationType));
            }

            migrations = migrations.OrderBy(x => x.Version).ToList();

            if (migrations.Select(x => x.Version).Distinct().Count() != migrations.Count)
            {
                throw new Exception("Duplicate versions detected.");
            }

            action(migrations);
        }
    }
}