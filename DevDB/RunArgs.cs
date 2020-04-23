using System.Data.Common;

namespace DevDB
{
    public class RunArgs
    {
        public RunCommand Command { get; set; }
        public RunOptions Options { get; set; }
    }

    public enum RunCommand
    {
        Reset,
        Migrate
    }

    public class RunOptions
    {
        public DbType DbType { get; set; }
        public DbConnectionStringBuilder Connection { get; set; }
        public string CustomPath { get; set; }
        public bool AlwaysYes { get; set; }
    }

    public enum DbType
    {
        Mssql,
        Pgsql
    }
}
