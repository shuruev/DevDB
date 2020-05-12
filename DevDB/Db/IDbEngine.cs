using System.Collections.Generic;
using DevDB.Reset;

namespace DevDB.Db
{
    public interface IDbEngine
    {
        public string ServerName { get; }
        public string DatabaseName { get; }

        public List<string> GetLogFilesToClean();
        public void DropAll(bool softReset);
        public void ExecuteCreation(ResetScript script, bool softReset);

        public int GetTableCount();
        public int GetProcedureCount();
    }
}
