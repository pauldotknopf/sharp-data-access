namespace SharpDataAccess
{
    public interface IDataAccessLogger
    {
        void Warn(string message);

        void Info(string message);
    }
}