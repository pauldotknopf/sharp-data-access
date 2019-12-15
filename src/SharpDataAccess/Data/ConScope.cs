using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

namespace SharpDataAccess.Data
{
    public class ConScope : IDisposable
    {
        private static readonly AsyncLocal<ContextData> _dbConnection = new AsyncLocal<ContextData>();
        private readonly bool _ownsScope;
        
        public ConScope(IDataService dataService)
        {
            if (_dbConnection.Value != null)
            {
                // There is already a previous connection.
                _ownsScope = false;
                _dbConnection.Value.IncrementChildCount();
            }
            else
            {
                _dbConnection.Value = new ContextData(dataService.OpenDbConnection(), dataService);
                _ownsScope = true;
            }
        }
        
        internal static IDbConnection InternalConnection
        {
            get
            {
                if (_dbConnection.Value == null)
                {
                    throw new Exception("No connection scope");
                }

                return _dbConnection.Value.Connection;
            }
        }

        internal static IDbTransaction InternalTransaction
        {
            get
            {
                if (_dbConnection.Value == null)
                {
                    throw new Exception("No connection scope");
                }

                return _dbConnection.Value.Transaction;
            }
        }
        
        public IDbConnection Connection => InternalConnection;

        public async Task<TransScope> BeginTransaction()
        {
            return await TransScope.Create();
        }
        
        internal static async Task<bool> RefTransaction()
        {
            if (_dbConnection.Value == null)
            {
                throw new Exception("No connection scope");
            }

            var connection = _dbConnection.Value;
            if (connection.Transaction == null)
            {
                await Task.Run(() => connection.Transaction = connection.DataService.OpenTransaction(connection.Connection));
                connection.IncrementTransactionCount();
                return true;
            }
            
            connection.IncrementTransactionCount();
            return false;
        }
        
        internal static void UnrefTransaction()
        {
            var connection = _dbConnection.Value;
            if (connection == null)
            {
                throw new Exception("No connection scope");
            }

            if (connection.TransactionCount == 0)
            {
                throw new Exception("Invalid transaction unref");
            }
            
            connection.DecrementTransactionCount();

            if (connection.TransactionCount == 0)
            {
                if (!connection.TransactionFinished)
                {
                    throw new Exception("Disposed of transaction scope without commiting or rolling back");
                }
                connection.Transaction.Dispose();
                connection.Transaction = null;
            }
        }

        public static bool HasOpenTransaction
        {
            get
            {
                var connection = _dbConnection.Value;
                if (connection == null)
                {
                    return false;
                }

                return connection.TransactionCount != 0;
            }
        }

        internal static void CommitTransaction()
        {
            var connection = _dbConnection.Value;
            if (connection == null)
            {
                throw new Exception("No connection scope");
            }

            if (connection.Transaction == null)
            {
                throw new Exception("No transaction detected");
            }

            if (connection.TransactionFinished)
            {
                throw new Exception("The transaction is finished");
            }
            
            connection.Transaction.Commit();
            connection.Commmited = true;
            connection.TransactionFinished = true;
        }

        internal static void RollbackTransaction()
        {
            var connection = _dbConnection.Value;
            if (connection == null)
            {
                throw new Exception("No connection scope");
            }

            if (connection.Transaction == null)
            {
                throw new Exception("No transaction detected");
            }

            if (connection.TransactionFinished)
            {
                throw new Exception("The transaction is finished");
            }
            
            connection.Transaction.Rollback();
            connection.RolledBack = true;
            connection.TransactionFinished = true;
        }

        public void Dispose()
        {
            var connection = _dbConnection.Value;
            if (_ownsScope)
            {
                if (connection.ChildCount > 0)
                {
                    throw new Exception("Child scopes were still detected.");
                }

                if (connection.TransactionCount > 0)
                {
                    throw new Exception("Transaction scopes are alive while connection is being disposed");
                }
                
                connection.Connection.Dispose();
                _dbConnection.Value = null;
            }
            else
            {
                connection.DecrementChildCount();
            }
        }

        internal class ContextData
        {
            public IDbConnection Connection;
            public IDbTransaction Transaction;
            public IDataService DataService;

            public int ChildCount;
            public int TransactionCount;

            public bool TransactionFinished;
            public bool Commmited;
            public bool RolledBack;
            
            public ContextData(IDbConnection connection,
                IDataService dataService)
            {
                Connection = connection;
                DataService = dataService;
            }

            public void IncrementChildCount()
            {
                ChildCount++;
            }

            public void DecrementChildCount()
            {
                ChildCount--;
            }

            public void IncrementTransactionCount()
            {
                TransactionCount++;
            }

            public void DecrementTransactionCount()
            {
                TransactionCount--;
            }
        }
    }
}