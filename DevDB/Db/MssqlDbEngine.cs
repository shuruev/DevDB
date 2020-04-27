using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using DevDB.Reset;

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

        public List<string> GetLogFilesToClean()
        {
            return Directory.GetFiles(_logPath, "*.sql")
                .Where(f => Path.GetFileName(f).StartsWith("Drop_"))
                .Where(f => Path.GetFileName(f).StartsWith("Create_"))
                .ToList();
        }

        public void DropAll() => ExecuteAndLog("Drop_All.sql", Scripts.Mssql.DropAll);

        public void ExecuteCreation(ResetScript script)
        {
            var all = Path.Combine(_logPath, "Create_All.sql");

            var sb = new StringBuilder();
            foreach (var file in script.Files)
            {
                sb.AppendLine($@"--------------------------------------------------
-- {file.BasePath}
--------------------------------------------------");

                sb.AppendLine(file.FileText);
                sb.AppendLine("GO");
                sb.AppendLine();
            }

            File.AppendAllText(all, sb.ToString());
            ExecuteAndLog($"Create_{script.ExecutionOrder:d2}_{script.CategoryName}.sql", sb.ToString());
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
