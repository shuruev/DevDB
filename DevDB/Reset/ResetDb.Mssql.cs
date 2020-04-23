using System;
using System.Data;
using System.Data.SqlClient;
using Atom.Util;

namespace DevDB.Reset
{
    partial class ResetDb
    {
        private void MssqlRunReset()
        {
            Verbose.WriteLine("Connecting to DB...");
            var x = MssqlExecuteScalar("SELECT * FROM sys.schemas");
            var y = MssqlExecuteNonQuery("SELECT * FROM sys.schemas");

            var scripts = BuildScripts();
            foreach (var script in scripts)
            {
                Console.WriteLine(script.GroupName);
                foreach (var file in script.Files)
                {
                    Console.WriteLine($"  - {file.FileName}");
                }
            }
        }

        private void MssqlDropAll()
        {
        }

        private object MssqlExecuteScalar(string sql)
        {
            using var conn = new SqlConnection(_options.Connection.ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            return cmd.ExecuteScalar();
        }

        private int MssqlExecuteNonQuery(string sql)
        {
            using var conn = new SqlConnection(_options.Connection.ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;

            return cmd.ExecuteNonQuery();
        }
    }
}
