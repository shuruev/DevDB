using System;

namespace DevDB.Db
{
    public class DbExecutorException : Exception
    {
        public string SqlScript { get; }
        public int LineNumber { get; }

        public DbExecutorException(string message, string sqlScript, int lineNumber, Exception innerException)
            : base(message, innerException)
        {
            SqlScript = sqlScript;
            LineNumber = lineNumber;
        }
    }
}
