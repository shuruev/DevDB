using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace DevDB.Db
{
    public class MssqlDbEngine : IDbEngine
    {
        private readonly SqlConnectionStringBuilder _connectionString;
        private readonly string _logPath;
        private readonly IDbExecutor _executor;

        public MssqlDbEngine(SqlConnectionStringBuilder connectionString, string logPath)
        {
            _connectionString = connectionString;
            _logPath = logPath;

            _executor = new MssqlAdoExecutor(_connectionString.ToString());
        }

        public string ServerName => _connectionString.DataSource;
        public string DatabaseName => _connectionString.InitialCatalog;

        public void CleanLogFiles()
        {
            DeleteLogFileIfExists("DropAll.sql");
        }

        private void DeleteLogFileIfExists(string logFile)
        {
            var filePath = Path.Combine(_logPath, logFile);
            if (!File.Exists(filePath))
                return;

            File.Delete(filePath);
        }

        public void DropAll() => ExecuteAndLog("DropAll.sql", Scripts.Mssql.DropAll);

        public void ExecuteScripts(IEnumerable<string> scripts)
        {
        }

        private void ExecuteAndLog(string fileName, string sqlScript)
        {
            var filePath = Path.Combine(_logPath, fileName);
            File.WriteAllText(filePath, sqlScript);

            _executor.ExecuteNonQuery(sqlScript);
        }

        public int GetTableCount() => _executor.ExecuteScalarInteger(Scripts.Mssql.GetTableCount);
        public int GetProcedureCount() => _executor.ExecuteScalarInteger(Scripts.Mssql.GetProcedureCount);
    }
}
