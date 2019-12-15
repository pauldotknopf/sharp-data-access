using System.Data;

namespace SharpDataAccess.Data
{
    public class DataOptions
    {
        public IsolationLevel? DefaultTransactionIsolationLevel { get; set; }
    }
}