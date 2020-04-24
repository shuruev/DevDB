using System;
using System.Collections.Generic;
using Npgsql;

namespace DevDB.Db
{
    public class PgsqlDbEngine : IDbEngine
    {
        private readonly NpgsqlConnectionStringBuilder _connectionString;
        private readonly IDbExecutor _executor;

        public PgsqlDbEngine(NpgsqlConnectionStringBuilder connectionString)
        {
            _connectionString = connectionString;
            _executor = null;
        }

        public string ServerName => _connectionString.Host;
        public string DatabaseName => _connectionString.Database;

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
