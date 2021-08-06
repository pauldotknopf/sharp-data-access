using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Sqlite;
using SharpDataAccess.Data;
using SharpDataAccess.Data.Impl;
using Xunit;

namespace SharpDataAccess.Tests
{
    public class UnitTest1
    {
        public class TestDbConnectionFactoryProvider : IDbConnectionFactoryProvider
        {
            public IDbConnectionFactory BuildConnectionFactory()
            {
                return new OrmLiteConnectionFactory(":memory:", new SqliteOrmLiteDialectProvider());
            }
        }
        
        [Fact]
        public async Task Test1()
        {
            var dataService = new DataService(new TestDbConnectionFactoryProvider(), null, new DataOptions());
            using (var con = new ConScope(await ConScope.GetAsyncContext(dataService)))
            {
                await Test(dataService);
            }
            
            using (var con = new ConScope(await ConScope.GetAsyncContext(dataService)))
            {
                await Test(dataService);
            }
        }

        public async Task Test(IDataService dataService)
        {
            using (var con2 = new ConScope(await ConScope.GetAsyncContext(dataService)))
            {
                    
            }
        }
    }
}
