using System;
using System.Collections.Generic;
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

        public void CleanLogFiles()
        {
            throw new NotImplementedException();
        }

        public void DropAll()
        {
            throw new NotImplementedException();
        }

        public void ExecuteScripts(IEnumerable<string> scripts)
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
