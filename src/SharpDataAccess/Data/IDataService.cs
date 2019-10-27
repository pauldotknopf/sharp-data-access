using System;
using System.Data;

namespace SharpDataAccess.Data
{
    public interface IDataService
    {
        IDbConnection OpenDbConnection();

        void WaitForDbConnection(TimeSpan timeout);
    }
}