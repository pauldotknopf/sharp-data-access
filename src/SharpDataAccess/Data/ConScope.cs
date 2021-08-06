using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.OrmLite;

namespace SharpDataAccess.Data
{
    public class ConScope : IDisposable
    {
        public static IDataScopeLogger DataScopeLogger { get; set; }
        
        private static readonly AsyncLocal<ContextData> _dbConnection = new AsyncLocal<ContextData>();
        private readonly bool _ownsScope;
        
        public ConScope(IDataService dataService)
        {
            if (_dbConnection.Value != null)
            {
                // There is already a previous connection.
                _ownsScope = false;
                _dbConnection.Value.IncrementChildCount();
                
                DataScopeLogger?.ScopeReused();
            }
            else
            {
                DataScopeLogger?.ScopeCreating();
                
                _dbConnection.Value = new ContextData(dataService.OpenDbConnection(), dataService);
                _ownsScope = true;
                
                DataScopeLogger?.ScopeCreated();
            }
        }

        public ConScope(InternalContext internalContext)
        {
            _ownsScope = internalContext.OwnsContext;
            _dbConnection.Value = internalContext.Context as ContextData;
        }

        internal ConScope(ContextData contextData, bool ownsScope)
        {
            _dbConnection.Value = contextData;
            _ownsScope = ownsScope;
        }

        public static async Task<InternalContext> GetAsyncContext(IDataService dataService)
        {
            if (_dbConnection.Value != null)
            {
                // There is already a previous connection.
                _dbConnection.Value.IncrementChildCount();
                
                DataScopeLogger?.ScopeReused();

                return new InternalContext
                {
                    Context = _dbConnection.Value,
                    OwnsContext = false
                };
            }
            else
            {
                DataScopeLogger?.ScopeCreating();
                
                _dbConnection.Value = new ContextData(dataService.OpenDbConnection(), dataService);
          
                DataScopeLogger?.ScopeCreated();
                
                return new InternalContext
                {
                    Context = _dbConnection.Value,
                    OwnsContext = true
                };
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
                DataScopeLogger?.TransactionCreating();
                
                await Task.Run(() => connection.Transaction = connection.DataService.OpenTransaction(connection.Connection));
                connection.IncrementTransactionCount();

                DataScopeLogger?.TransactionCreated();
                
                return true;
            }
            
            connection.IncrementTransactionCount();
            
            DataScopeLogger?.TransactionReused();
            
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

                DataScopeLogger?.TransactionDestroying();
                
                connection.Transaction.Dispose();
                connection.Transaction = null;
                
                DataScopeLogger?.TransactionDestroyed();
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
                
                DataScopeLogger?.ScopeDestroying();
                
                connection.Connection.Dispose();
                _dbConnection.Value = null;
                
                DataScopeLogger?.ScopeDestroyed();
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
        
        public sealed class InternalContext
        {
            public bool OwnsContext { get; set; }
        
            public object Context { get; set; }
        }
    }
}