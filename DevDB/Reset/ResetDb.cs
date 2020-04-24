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
        private readonly RunOptions _options;

        public ResetDb(RunOptions options)
        {
            _options = options;
        }

        public void Run()
        {
            var engine = GetDbEngine();

            if (!Validate(engine))
                return;

            if (!Prompt(engine))
                return;

            XConsole.NewPara();

            var scripts = BuildScripts();
            if (scripts == null)
                return;

            var sw = Stopwatch.StartNew();

            XConsole.Write("Drop all objects... ");
            engine.DropAll();
            XConsole.Green.WriteLine("OK");

            foreach (var script in scripts)
            {
                foreach (var file in script.Files)
                    Verbose.WriteLine($"{script.GroupName}: {file.FileName}");

                XConsole.Write($"Creating {script.GroupName}... ");
                engine.ExecuteScripts(script.Files.Select(f => f.FileText));
                XConsole.Green.WriteLine("OK");
            }

            sw.Stop();
            Verbose.WriteLine($"Database reset took {sw.Elapsed}");

            Report(engine);
        }

        private IDbEngine GetDbEngine() => _options.DbType switch
        {
            DbType.Mssql => new MssqlDbEngine((SqlConnectionStringBuilder)_options.Connection),
            DbType.Pgsql => new PgsqlDbEngine((NpgsqlConnectionStringBuilder)_options.Connection),
            _ => throw new InvalidOperationException($"DB type {_options.DbType} is not supported yet")
        };

        private bool Validate(IDbEngine engine)
        {
            if (_options.Connection == null)
            {
                XConsole.NewPara().Warning.WriteLine("Connection is not set");
                XConsole.Write("Use -c to specify connection, e.g. ").Yellow.WriteLine("-c \"Server=localhost; Database=MyDb; Integrated Security=True;\"");
                return false;
            }


            if (String.IsNullOrWhiteSpace(engine.ServerName))
            {
                XConsole.NewPara().Warning.WriteLine("DB server is not set");
                XConsole.Write("Specify 'Server' in your connection, e.g. -c \"").Yellow.Write("Server=localhost;").Default.Write(" Database=MyDb; ...\"");
                return false;
            }

            if (String.IsNullOrWhiteSpace(engine.DatabaseName))
            {
                XConsole.NewPara().Warning.WriteLine("DB name is not set");
                XConsole.Write("Specify 'Database' in your connection, e.g. -c \"Server=localhost; ").Yellow.Write("Database=MyDb;").Default.Write(" ...\"");
                return false;
            }

            return true;
        }

        private bool Prompt(IDbEngine engine)
        {
            XConsole.NewPara().Write("Performing reset for ").Cyan.Write(engine.DatabaseName).Default.Write(" database at ").Cyan.WriteLine(engine.ServerName);
            XConsole.Write("This will ").Error.Write("*** ERASE ***").Default.Write(" your local DB and recreate it from scripts. You sure (Y/N)? ");

            if (_options.AlwaysYes)
            {
                Console.Write('y');
                return true;
            }

            var key = Console.ReadKey();
            return key.Key == ConsoleKey.Y;
        }

        private void Report(IDbEngine engine)
        {
            var sw = Stopwatch.StartNew();
            var tables = engine.GetTableCount();
            var procedures = engine.GetProcedureCount();
            sw.Stop();

            XConsole.NewPara().Write("Database ").Cyan.Write(engine.DatabaseName).Default.Write(" at ").Cyan.Write(engine.ServerName).Default.WriteLine(" was successfully reset.");
            XConsole.Write("Now it has ").Yellow.Write($"{tables}").Default.Write(" tables and ").Yellow.Write($"{procedures}").Default.WriteLine(" procedures in case you ever wondered.");
            Verbose.WriteLine($"Getting stats took {sw.Elapsed}");
        }

        private List<ResetScript> BuildScripts()
        {
            var path = Environment.CurrentDirectory;
            if (!String.IsNullOrWhiteSpace(_options.CustomPath))
                path = _options.CustomPath;

            Verbose.WriteLine($"Target path: {path}");
            Verbose.WriteLine("Locating script files...");

            var files = Directory.GetFiles(path, "*.sql", SearchOption.AllDirectories);
            Verbose.WriteLine($"{files.Length} files found");

            var scripts = files
                .GroupBy(f => GetScriptGroupName(f, path))
                .Select(g => new ResetScript
                {
                    GroupName = g.Key,
                    Files = g
                        .Select(f => new ResetScriptFile
                        {
                            FileName = f,
                            FileText = File.ReadAllText(f)
                        })
                        .ToList()
                })
                .ToList();

            if (scripts.Count == 0)
            {
                XConsole.NewPara().Warning.WriteLine("No SQL scripts found");
                XConsole.Write("No SQL scripts could be found at ").Cyan.WriteLine(path);
                XConsole.WriteLine("If this was not intended to be using current path, use -p to specify custom path");
                XConsole.Write("e.g. ").Yellow.WriteLine("-p \"C:\\MyRepos\\MyProject\\database\"");
                return null;
            }

            return scripts;
        }

        private static string GetScriptGroupName(string fileName, string basePath)
        {
            var file = Path.GetRelativePath(basePath, fileName);

            var parts = file.Split('_', '-', '/', '\\');
            if (parts.Length == 1)
                return "Other";

            return parts[0];
        }
    }
}
