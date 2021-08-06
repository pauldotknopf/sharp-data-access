using System;
using System.Data;
using System.Threading.Tasks;

namespace SharpDataAccess.Data
{
    public interface IDataService
    {
        IDbConnection OpenDbConnection();

        Task<IDbConnection> OpenDbConnectionAsync();

        IDbTransaction OpenTransaction(IDbConnection connection);
        
        void WaitForDbConnection(TimeSpan timeout);
    }
}