namespace DevDB.Db
{
    public interface IDbExecutor
    {
        public void ExecuteNonQuery(string sql);
        public int ExecuteScalarInteger(string sql);
        public string ExecuteScalarString(string sql);
    }
}
