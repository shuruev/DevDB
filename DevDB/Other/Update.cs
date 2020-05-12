using System;
using System.IO;
using Atom.Util;

namespace DevDB.Other
{
    public class Update
    {
        private const string FILE_NAME = "updated.txt";
        private const int UPDATE_EVERY_DAYS = 5;

        private readonly RunContext _context;

        public Update(RunContext context)
        {
            _context = context;
        }

        public void Run()
        {
            XConsole.NewPara();

            var updatedFile = Path.Combine(_context.LogPath, FILE_NAME);

            Verbose.WriteLine("Checking if update is needed...");
            if (File.Exists(updatedFile))
            {
                var updatedTime = File.GetCreationTimeUtc(updatedFile);
                if (DateTime.UtcNow.Subtract(updatedTime).TotalDays < UPDATE_EVERY_DAYS)
                {
                    Verbose.WriteLine("Update is not required");
                    return;
                }
            }

            Verbose.WriteLine($"Update is required, so {FILE_NAME} file will be deleted");
            File.Delete(updatedFile);
        }
    }
}
