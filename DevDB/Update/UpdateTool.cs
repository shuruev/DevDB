using System;
using System.IO;

namespace DevDB.Update
{
    public class UpdateTool
    {
        private const int UPDATE_EVERY_DAYS = 5;

        private readonly RunContext _context;

        public UpdateTool(RunContext context)
        {
            _context = context;
        }

        public void Run()
        {
            //xxx

            // check for updates
            /*var updated = Path.Combine(_logPath, "updated.txt");
            if (File.Exists(updated))
            {
                var updatedTime = File.GetCreationTimeUtc(updated);
                if (DateTime.UtcNow.Subtract(updatedTime).TotalDays > UPDATE_EVERY_DAYS)
                {
                    File.Delete(updated);
                    Verbose.WriteLine("Need to check for updates, use updated.txt to detect");
                }
            }*/
        }
    }
}
