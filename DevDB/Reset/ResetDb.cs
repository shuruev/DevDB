using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Atom.Util;
using DevDB.Db;
using Npgsql;

namespace DevDB.Reset
{
    public class ResetDb
    {
        private readonly RunContext _context;

        public ResetDb(RunContext context)
        {
            _context = context;
        }

        public void Run()
        {
            if (!Validate())
                return;

            var engine = GetDbEngine();

            if (!Validate(engine))
                return;

            if (!Proceed(engine))
                return;

            // prepare scripts
            XConsole.NewPara();
            CleanLogFiles(engine);

            var scripts = BuildScripts();
            if (scripts == null)
                return;

            var sw = Stopwatch.StartNew();

            // run scripts
            XConsole.NewPara();
            Execute($"Drop {(_context.UseSoftReset ? "only programmatic" : "all")} objects...", () => engine.DropAll(_context.UseSoftReset));

            foreach (var script in scripts)
            {
                Verbose.WriteLine();
                foreach (var file in script.Files)
                    Verbose.WriteLine($"{script.CategoryName.ToUpper()} : {file.BasePath}");

                if (_context.UseSoftReset && !KnownScripts.UsedWhenSoftReset(script.CategoryName))
                {
                    XConsole.Write($"Creating {script.CategoryName.ToLower()}... ").Gold.Write("Skipped");
                    Verbose.Write(" (due to \"soft\" reset)");
                    XConsole.WriteLine();
                    continue;
                }

                Execute($"Creating {script.CategoryName.ToLower()}...", () => engine.ExecuteCreation(script, _context.UseSoftReset));
            }

            sw.Stop();
            Verbose.WriteLine();
            Verbose.WriteLine($"Database reset took {sw.ElapsedMilliseconds:N0} ms");

            Report(engine);
        }

        private IDbEngine GetDbEngine() => _context.DbType switch
        {
            DbType.Mssql => new MssqlDbEngine((SqlConnectionStringBuilder)_context.Connection, _context.LogPath),
            DbType.Pgsql => new PgsqlDbEngine((NpgsqlConnectionStringBuilder)_context.Connection, _context.LogPath),
            _ => throw new InvalidOperationException($"DB type {_context.DbType} is not supported yet")
        };

        private bool Validate()
        {
            if (_context.Connection == null)
            {
                XConsole.NewPara().Warning.WriteLine("Connection is not set");
                XConsole.Write("Specify connection after 'reset' command, e.g. ").Yellow.WriteLine("reset \"Server=localhost; Database=MyDb; Integrated Security=True;\"");
                return false;
            }

            return true;
        }

        private static bool Validate(IDbEngine engine)
        {
            if (String.IsNullOrWhiteSpace(engine.ServerName))
            {
                XConsole.NewPara().Warning.WriteLine("DB server is not set");
                XConsole.Write("Specify 'Server' in your connection, e.g. \"").Yellow.Write("Server=localhost;").Default.Write(" Database=MyDb; ...\"");
                return false;
            }

            if (String.IsNullOrWhiteSpace(engine.DatabaseName))
            {
                XConsole.NewPara().Warning.WriteLine("DB name is not set");
                XConsole.Write("Specify 'Database' in your connection, e.g. \"Server=localhost; ").Yellow.Write("Database=MyDb;").Default.Write(" ...\"");
                return false;
            }

            return true;
        }

        private bool Proceed(IDbEngine engine)
        {
            if (_context.UseSoftReset)
            {
                XConsole.NewPara().Write("Performing ").Gold.Write("\"soft\"").Default.Write(" reset for ").Cyan.Write(engine.DatabaseName).Default.Write(" database at ").Cyan.WriteLine(engine.ServerName);
                XConsole.Write("This will ").Warning.Write("*** ERASE ***").Default.WriteLine(" only programmatic objects (procedures, functions, views, etc.) in your DB and will attempt to");
                XConsole.Write("recreate them from scripts. You sure (Y/N)? ");
            }
            else
            {
                XConsole.NewPara().Write("Performing reset for ").Cyan.Write(engine.DatabaseName).Default.Write(" database at ").Cyan.WriteLine(engine.ServerName);
                XConsole.Write("This will ").Error.Write("*** ERASE ***").Default.Write(" your DB and recreate it from scripts. You sure (Y/N)? ");
            }

            var proceed = Prompt.YesNo();
            if (!proceed)
                XConsole.NewPara().WriteLine("Reset aborted, exiting...");

            return proceed;
        }

        private static void Execute(string log, Action action)
        {
            XConsole.Write($"{log} ");

            var sw = Stopwatch.StartNew();
            action.Invoke();
            sw.Stop();

            XConsole.Green.Write("OK");
            Verbose.Write($" ({sw.ElapsedMilliseconds:N0} ms)");
            XConsole.WriteLine();
        }

        private static void Report(IDbEngine engine)
        {
            var sw = Stopwatch.StartNew();
            var tables = engine.GetTableCount();
            var procedures = engine.GetProcedureCount();
            sw.Stop();

            XConsole.NewPara().Write("Database ").Cyan.Write(engine.DatabaseName).Default.Write(" at ").Cyan.Write(engine.ServerName).Default.WriteLine(" was successfully reset.");
            XConsole.Write("Now it has ").Yellow.Write($"{tables}").Default.Write(" tables and ").Yellow.Write($"{procedures}").Default.WriteLine(" procedures (in case you ever wondered)");
            Verbose.WriteLine($"Getting stats took {sw.ElapsedMilliseconds:N0} ms");
        }

        private static void CleanLogFiles(IDbEngine engine)
        {
            var existing = engine.GetLogFilesToClean();

            foreach (var file in existing)
            {
                File.Delete(file);
            }

            if (existing.Count > 0)
                Verbose.WriteLine($"Deleted {existing.Count} log files");
        }

        private List<ResetScript> BuildScripts()
        {
            Verbose.WriteLine("Locating script files...");

            var files = Directory.GetFiles(_context.TargetPath, "*.sql", SearchOption.AllDirectories);
            Verbose.WriteLine($"Found {files.Length} *.sql files");

            // filter out SQL scripts to use
            var all = files
                .GroupBy(f => GetScriptGroupName(f, _context.TargetPath))
                .Select(g => new ResetScript
                {
                    CategoryName = g.Key,
                    Files = g
                        .Select(f => new ResetScriptFile
                        {
                            FullPath = f,
                            BasePath = Path.GetRelativePath(_context.TargetPath, f),
                            FileText = File.ReadAllText(f)
                        })
                        .OrderBy(f => f.FullPath)
                        .ToList()
                })
                .ToList();

            var scripts = all
                .Where(i => i.CategoryName != null)
                .OrderBy(s => KnownScripts.GetCategoryOrder(s.CategoryName))
                .ToList();

            for (var i = 0; i < scripts.Count; i++)
            {
                scripts[i].ExecutionOrder = i + 1;
            }

            // display ignored files
            var ignored = all.FirstOrDefault(i => i.CategoryName == null);
            if (ignored != null)
            {
                if (ignored.Files.Count > 0)
                {
                    Verbose.WriteLine($"Ignored {ignored.Files.Count} files:");
                    foreach (var file in ignored.Files)
                    {
                        Verbose.WriteLine($"- {file.BasePath}");
                    }
                }
            }

            if (scripts.Count == 0)
            {
                XConsole.NewPara().Warning.WriteLine("No SQL scripts found");
                XConsole.Write("No SQL scripts could be found at ").Cyan.WriteLine(_context.TargetPath);
                XConsole.WriteLine("If this was not intended to be using current path, use -p to specify custom path");
                XConsole.Write("e.g. ").Yellow.WriteLine("-p \"C:\\MyRepos\\MyProject\\database\"");
                return null;
            }

            Verbose.WriteLine($"Filtered out {scripts.Sum(s => s.Files.Count)} scripts to run, in {scripts.Count} categories");
            return scripts;
        }

        private static string GetScriptGroupName(string fileName, string basePath)
        {
            var file = Path.GetRelativePath(basePath, fileName);

            var parts = file.Split('_', '-', '/', '\\', '.');
            return KnownScripts.Categorize(parts[0]);
        }
    }
}
