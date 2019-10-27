using System;
using System.Collections.Generic;

namespace SharpDataAccess.Migrations
{
    public interface IMigrationsBuilder
    {
        void BuildMigrations(Action<IList<IMigration>> action);
    }
}