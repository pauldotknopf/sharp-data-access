using ServiceStack.Data;

namespace SharpDataAccess.Data
{
    public interface IDbConnectionFactoryProvider
    {
        IDbConnectionFactory BuildConnectionFactory();
    }
}