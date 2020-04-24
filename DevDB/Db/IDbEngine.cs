using System.Collections.Generic;

namespace DevDB.Db
{
    public interface IDbEngine
    {
        public string ServerName { get; }
        public string DatabaseName { get; }

        public void CleanLogFiles();
        public void DropAll();
        public void ExecuteScripts(IEnumerable<string> scripts);

        public int GetTableCount();
        public int GetProcedureCount();
    }
}
