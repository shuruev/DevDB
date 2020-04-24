using System.Collections.Generic;
using System.Data.SqlClient;

namespace DevDB.Db
{
    public class MssqlDbEngine : IDbEngine
    {
        private readonly SqlConnectionStringBuilder _connectionString;
        private readonly IDbExecutor _executor;

        public MssqlDbEngine(SqlConnectionStringBuilder connectionString)
        {
            _connectionString = connectionString;
            _executor = new MssqlAdoExecutor(_connectionString.ToString());
        }

        public string ServerName => _connectionString.DataSource;
        public string DatabaseName => _connectionString.InitialCatalog;

        public void DropAll()
        {
        }

        public void ExecuteScripts(IEnumerable<string> scripts)
        {
        }

        public int GetTableCount()
        {
            return _executor.ExecuteScalarInteger(@"
SELECT COUNT(*)
FROM sys.tables T
	LEFT JOIN sys.extended_properties EP
	ON EP.major_id = T.[object_id]
WHERE
	EP.class_desc IS NULL
	OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support')
");
        }

        public int GetProcedureCount()
        {
            return _executor.ExecuteScalarInteger(@"
SELECT COUNT(*)
FROM sys.procedures P
	LEFT JOIN sys.extended_properties EP
	ON EP.major_id = P.[object_id]
WHERE
	EP.class_desc IS NULL
	OR (EP.class_desc <> 'OBJECT_OR_COLUMN' AND EP.[name] <> 'microsoft_database_tools_support')
");
        }
    }
}
