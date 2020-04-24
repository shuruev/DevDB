using System.Data;
using System.Data.SqlClient;

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
            using var cmd = NewCommand(conn, sql);
            cmd.ExecuteNonQuery();
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
