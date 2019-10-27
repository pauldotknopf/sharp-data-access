using System;
using SharpDataAccess.Data;

namespace SharpDataAccess.Impl
{
    public class ConsoleDataAccessLogger : IDataAccessLogger
    {
        public void Warn(string message)
        {
            Console.WriteLine(message);
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}