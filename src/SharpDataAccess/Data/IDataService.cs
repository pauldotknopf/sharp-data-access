using System;
using System.Data;

namespace SharpDataAccess.Data
{
    public interface IDataService
    {
        IDbConnection OpenDbConnection();

        IDbTransaction OpenTransaction(IDbConnection connection);
        
        void WaitForDbConnection(TimeSpan timeout);
    }
}