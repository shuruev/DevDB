using System.Data.Common;

namespace DevDB
{
    public class RunArgs
    {
        public RunCommand Command { get; set; }
        public DbType DbType { get; set; }
        public DbConnectionStringBuilder Connection { get; set; }
        public string CustomPath { get; set; }
        public bool UseSoftReset { get; set; }
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

    public class RunContext
    {
        public DbType DbType { get; set; }
        public DbConnectionStringBuilder Connection { get; set; }
        public string TargetPath { get; set; }
        public string LogPath { get; set; }
        public bool UseSoftReset { get; set; }
    }
}
