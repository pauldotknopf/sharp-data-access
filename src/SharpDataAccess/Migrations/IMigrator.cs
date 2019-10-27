namespace SharpDataAccess.Migrations
{
    public interface IMigrator
    {
        void Migrate();

        int? GetCurrentVersion();
    }
}