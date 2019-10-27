using System;
using System.Collections.Generic;

namespace SharpDataAccess.Migrations
{
    public interface IMigrationTypesProvider
    {
        IList<Type> GetMigrationTypes();
    }
}