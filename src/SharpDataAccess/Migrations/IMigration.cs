using System.Data;

namespace SharpDataAccess.Migrations
{
    public interface IMigration
    {
        void Run(IDbConnection connection);
        
        int Version { get; }
    }
}