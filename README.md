# SharpDataAccess

![SharpDataAccess](https://img.shields.io/nuget/v/SharpDataAccess.svg?style=flat&label=SharpDataAccess)

A .NET library for common database functionality (migrations, ambient connection/transactions)

## About

This library provides a small set of utilities that are typically needed when implementing a bare-metal data-access solution, using ServiceStack.OrmLite.

There isn't much to this library, I just hated implementing repetitive types in all my projects.

## Example usage

```csharp
public class ExampleUsage
{
    public async Task Example()
    {
        // Build a service that will allow us to interact with the database.
        IDbConnectionFactory dbConnectionFactory = null; // see ServiceStack.OrmLite documentation
        IDataAccessLogger logger = new SharpDataAccess.Impl.ConsoleDataAccessLogger(); // Re-implement to plug into your logging system.
        var dataService = new SharpDataAccess.Data.Impl.DataService(dbConnectionFactory, logger);
        
        // Wait for the database to be available. This is ideal for docker scenarios, where the DB isn't immediately available.
        dataService.WaitForDbConnection(TimeSpan.FromSeconds(30));
        
        // Migration your database.
        IMigrationsBuilder migrationBuilder = null; // Implement this service to discover/instantiate your migrations.
        var migrator = new Migrator(dataService, migrationBuilder, logger);
        migrator.Migrate();
        
        // Access your data
        using (var connectionScope = new ConScope(dataService))
        {
            // Here is a raw connection.
            IDbConnection connection = connectionScope.Connection;

            using (var transactionScope = await connectionScope.BeginTransaction())
            {
                await DataMethod(dataService);
                
                transactionScope.Commit();
            }
        }
    }

    private async Task DataMethod(IDataService dataService)
    {
        using (var connectionScope = new ConScope(dataService))
        {
            // This is the same exact connection from the outer method call.
            IDbConnection connection = connectionScope.Connection;

            using (var transactionScope = await connectionScope.BeginTransaction())
            {
                // This is the same transaction from the outer method call.
                transactionScope.Commit();
            }
        }
    }
}
```