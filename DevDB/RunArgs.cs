using System.Data.Common;

namespace DevDB
{
    public class RunArgs
    {
        public RunCommand Command { get; set; }
        public DbType DbType { get; set; }
        public DbConnectionStringBuilder Connection { get; set; }
    }

    public enum RunCommand
    {
        Reset,
        Migrate
    }

    public enum DbType
    {
        Mssql,
        Pgsql
    }
}
