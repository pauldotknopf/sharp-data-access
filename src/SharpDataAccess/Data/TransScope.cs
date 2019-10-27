using System;
using System.Threading.Tasks;

namespace SharpDataAccess.Data
{
    public class TransScope : IDisposable
    {
        private bool _ownsTransaction;
        private bool _commited;
        private bool _rolledBack;
        private bool _finished;
        private bool _disposed;
        
        private TransScope()
        {
            
        }

        private async Task Init()
        {
            _ownsTransaction = await ConScope.RefTransaction();
        }

        internal static async Task<TransScope> Create()
        {
            var transScope = new TransScope();
            await transScope.Init();
            return transScope;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_ownsTransaction)
            {
                if (!_finished)
                {
                    // If we disposed transaction without commit/rollback, do an implicit rollback.
                    ConScope.RollbackTransaction();
                }
            }
            
            ConScope.UnrefTransaction();
        }

        public void Commit()
        {
            if (_finished)
            {
                throw new Exception("This transaction is finished");
            }
            if (!_commited)
            {
                if (_ownsTransaction)
                {
                    ConScope.CommitTransaction();
                }
                _commited = true;
                _finished = true;
            }
        }

        public void Rollback()
        {
            if (_finished)
            {
                throw new Exception("This transaction is finished");
            }
            if (!_rolledBack)
            {
                if (_ownsTransaction)
                {
                    ConScope.RollbackTransaction();
                }
                _rolledBack = true;
                _finished = true;
            }
        }
        
        public bool OwnsTransaction => _ownsTransaction;
    }
}