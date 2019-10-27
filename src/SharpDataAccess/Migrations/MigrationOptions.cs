using System;
using System.Reflection;

namespace SharpDataAccess.Migrations
{
    public class MigrationOptions
    {
        public MigrationOptions(Assembly migrationsAssembly)
        {
            if (migrationsAssembly == null)
            {
                throw new Exception("You must provide a migrations assembly");
            }
            
            MigrationsAssembly = migrationsAssembly;
        }
        
        public Assembly MigrationsAssembly { get; }
    }
}