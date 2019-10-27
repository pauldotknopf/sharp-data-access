using System;
using System.Data;
using System.Threading;
using ServiceStack.Data;

namespace SharpDataAccess.Data.Impl
{
    public class DataService : IDataService
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;
        private readonly IDataAccessLogger _logger;

        public DataService(IDbConnectionFactoryProvider dbConnectionFactoryProvider, IDataAccessLogger logger)
        {
            _dbConnectionFactory = dbConnectionFactoryProvider.BuildConnectionFactory();
            _logger = logger;
        }
        
        public IDbConnection OpenDbConnection()
        {
            return _dbConnectionFactory.OpenDbConnection();
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