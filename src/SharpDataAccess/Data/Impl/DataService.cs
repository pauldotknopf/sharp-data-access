using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Data;
using ServiceStack.OrmLite;

namespace SharpDataAccess.Data.Impl
{
    public class DataService : IDataService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IDataAccessLogger _logger;
        private readonly DataOptions _options;

        public DataService(
            IDbConnectionFactoryProvider dbConnectionFactoryProvider,
            IDataAccessLogger logger,
            DataOptions options)
        {
            _dbConnectionFactory = dbConnectionFactoryProvider.BuildConnectionFactory();
            _logger = logger;
            _options = options;
        }
        
        public IDbConnection OpenDbConnection()
        {
            return _dbConnectionFactory.OpenDbConnection();
        }

        public async Task<IDbConnection> OpenDbConnectionAsync()
        {
            return await _dbConnectionFactory.OpenDbConnectionAsync();
        }

        public IDbTransaction OpenTransaction(IDbConnection connection)
        {
            var isolationLevel = _options.DefaultTransactionIsolationLevel;
            if (isolationLevel.HasValue)
            {
                return connection.OpenTransaction(isolationLevel.Value);
            }
            return connection.OpenTransaction();
        }

        public void WaitForDbConnection(TimeSpan timeout)
        {
            using (var cancel = new CancellationTokenSource())
            {
                cancel.CancelAfter(timeout);

                bool isFirst = true;
                while (true)
                {
                    try
                    {
                        using (OpenDbConnection())
                        {
                            return;
                        }
                    }
                    catch (Exception)
                    {
                        if (isFirst)
                        {
                            _logger.Warn("Waiting for DB...");
                        }

                        if (cancel.IsCancellationRequested)
                        {
                            throw;
                        }
                    }

                    isFirst = false;
                }
            }
        }
    }
}