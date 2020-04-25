using System.Collections.Generic;

namespace DevDB.Reset
{
    public class ResetScript
    {
        public string CategoryName { get; set; }
        public int ExecutionOrder { get; set; }
        public List<ResetScriptFile> Files { get; set; }
    }

    public class ResetScriptFile
    {
        public string FullPath { get; set; }
        public string BasePath { get; set; }
        public string FileText { get; set; }
    }
}
