using System;
using System.Collections.Generic;
using DevDB.Reset;
using Npgsql;

namespace DevDB.Db
{
    public class PgsqlDbEngine : IDbEngine
    {
        private readonly NpgsqlConnectionStringBuilder _connectionString;
        private readonly string _logPath;
        private readonly IDbExecutor _executor;

        public PgsqlDbEngine(NpgsqlConnectionStringBuilder connectionString, string logPath)
        {
            _connectionString = connectionString;
            _logPath = logPath;

            _executor = null;
        }

        public string ServerName => _connectionString.Host;
        public string DatabaseName => _connectionString.Database;

        public List<string> GetLogFilesToClean()
        {
            throw new NotImplementedException();
        }

        public void DropAll(bool softReset)
        {
            throw new NotImplementedException();
        }

        public void ExecuteCreation(ResetScript script, bool softReset)
        {
            throw new NotImplementedException();
        }

        public int GetTableCount()
        {
            throw new NotImplementedException();
        }

        public int GetProcedureCount()
        {
            throw new NotImplementedException();
        }
    }
}
