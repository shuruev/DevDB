using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DevDB.Db
{
    public class MssqlAdoExecutor : IDbExecutor
    {
        private readonly string _connectionString;

        public MssqlAdoExecutor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void ExecuteNonQuery(string sql)
        {
            using var conn = OpenConnection();

            foreach (var batch in SplitByGo(sql))
            {
                if (String.IsNullOrWhiteSpace(batch))
                    continue;

                using var cmd = NewCommand(conn, batch);
                cmd.ExecuteNonQuery();
            }
        }

        private IEnumerable<string> SplitByGo(string sql)
        {
            var lines = sql
                .Replace("\r\n", "\n")
                .Replace("\r", "\n")
                .Split('\n');

            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (line.Trim(' ', '\t', ';').ToUpper() == "GO")
                {
                    var batch = sb.ToString();
                    sb.Clear();
                    yield return batch;
                    continue;
                }

                sb.AppendLine(line);
            }

            yield return sb.ToString();
        }

        public int ExecuteScalarInteger(string sql) => (int)ExecuteScalar(sql);
        public string ExecuteScalarString(string sql) => (string)ExecuteScalar(sql);

        private object ExecuteScalar(string sql)
        {
            using var conn = OpenConnection();
            using var cmd = NewCommand(conn, sql);
            return cmd.ExecuteScalar();
        }

        private SqlConnection OpenConnection()
        {
            var conn = new SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private SqlCommand NewCommand(SqlConnection conn, string sql)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = CommandType.Text;
            return cmd;
        }
    }
}
