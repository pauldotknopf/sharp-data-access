using System;

namespace SharpDataAccess.Migrations
{
    public interface IMigrationBuilder
    {
        IMigration Create(Type migrationType);
    }
}