using System;
using System.Collections.Generic;
using System.Linq;

namespace SharpDataAccess.Migrations.Impl
{
    public class MigrationTypesProvider : IMigrationTypesProvider
    {
        private readonly MigrationOptions _migrationOptions;

        public MigrationTypesProvider(MigrationOptions migrationOptions)
        {
            _migrationOptions = migrationOptions;
        }
        
        public IList<Type> GetMigrationTypes()
        {
            var result = new List<Type>();
            
            foreach (var type in _migrationOptions.MigrationsAssembly.GetTypes())
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }
                
                var migrationAttribute = type.GetCustomAttributes(typeof(MigrationAttribute), false)
                    .OfType<MigrationAttribute>()
                    .SingleOrDefault();

                if (migrationAttribute == null)
                {
                    continue;
                }
                
                result.Add(type);
            }

            return result;
        }
    }
}