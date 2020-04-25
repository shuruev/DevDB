﻿using System;
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
        private const int UPDATE_EVERY_DAYS = 5;

        private readonly RunOptions _options;

        private string _targetPath;
        private string _logPath;

        public ResetDb(RunOptions options)
        {
            _options = options;
        }

        public void Run()
        {
            if (!InitializeFolders())
                return;

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

            XConsole.NewPara();
            Execute("Drop all objects...", () => engine.DropAll());

            foreach (var script in scripts)
            {
                Verbose.WriteLine();
                foreach (var file in script.Files)
                    Verbose.WriteLine($"{script.CategoryName.ToUpper()} : {file.BasePath}");

                Execute($"Creating {script.CategoryName.ToLower()}...", () => engine.ExecuteCreation(script));
            }

            sw.Stop();
            Verbose.WriteLine();
            Verbose.WriteLine($"Database reset took {sw.ElapsedMilliseconds:N0} ms");

            Report(engine);
        }

        private IDbEngine GetDbEngine() => _options.DbType switch
        {
            DbType.Mssql => new MssqlDbEngine((SqlConnectionStringBuilder)_options.Connection, _logPath),
            DbType.Pgsql => new PgsqlDbEngine((NpgsqlConnectionStringBuilder)_options.Connection, _logPath),
            _ => throw new InvalidOperationException($"DB type {_options.DbType} is not supported yet")
        };

        private bool InitializeFolders()
        {
            XConsole.NewPara();

            // choose target path
            _targetPath = Environment.CurrentDirectory;
            if (!String.IsNullOrWhiteSpace(_options.CustomPath))
                _targetPath = _options.CustomPath;

            if (!Directory.Exists(_targetPath))
            {
                XConsole.NewPara().Warning.WriteLine("Cannot locate target path");
                XConsole.Write("The following target path was not found: ").Cyan.WriteLine(_targetPath);
                XConsole.Write("When you use ").Yellow.Write("-p").Default.WriteLine(" parameter to specify custom path, make sure it points to existing location");
                return false;
            }

            Verbose.WriteLine($"Target path: {_targetPath}");

            // get log path
            _logPath = Path.Combine(_targetPath, "log");
            Verbose.Write($"Log path: {_logPath}");

            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
                Verbose.Write(" (created)");
            }

            Verbose.WriteLine();

            // clean log files
            var existing = Directory.GetFiles(_logPath, "*.*")
                .Where(f => Path.GetFileName(f) != "updated.txt")
                .ToArray();

            foreach (var file in existing)
            {
                File.Delete(file);
            }

            if (existing.Length > 0)
                Verbose.WriteLine($"Deleted {existing.Length} log files");

            // check for updates
            var updated = Path.Combine(_logPath, "updated.txt");
            if (File.Exists(updated))
            {
                var updatedTime = File.GetCreationTimeUtc(updated);
                if (DateTime.UtcNow.Subtract(updatedTime).TotalDays > UPDATE_EVERY_DAYS)
                {
                    File.Delete(updated);
                    Verbose.WriteLine("Need to check for updates, use updated.txt to detect");
                }
            }

            return true;
        }

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
            return key.Key == ConsoleKey.Y
                || key.Key == ConsoleKey.D1
                || key.Key == ConsoleKey.NumPad1;
        }

        private void Execute(string log, Action action)
        {
            XConsole.Write($"{log} ");

            var sw = Stopwatch.StartNew();
            action.Invoke();
            sw.Stop();

            XConsole.Green.Write("OK");
            Verbose.Write($" ({sw.ElapsedMilliseconds:N0} ms)");
            XConsole.WriteLine();
        }

        private void Report(IDbEngine engine)
        {
            var sw = Stopwatch.StartNew();
            var tables = engine.GetTableCount();
            var procedures = engine.GetProcedureCount();
            sw.Stop();

            XConsole.NewPara().Write("Database ").Cyan.Write(engine.DatabaseName).Default.Write(" at ").Cyan.Write(engine.ServerName).Default.WriteLine(" was successfully reset.");
            XConsole.Write("Now it has ").Yellow.Write($"{tables}").Default.Write(" tables and ").Yellow.Write($"{procedures}").Default.WriteLine(" procedures (in case you ever wondered)");
            Verbose.WriteLine($"Getting stats took {sw.ElapsedMilliseconds:N0} ms");
        }

        private List<ResetScript> BuildScripts()
        {
            Verbose.WriteLine("Locating script files...");

            var files = Directory.GetFiles(_targetPath, "*.sql", SearchOption.AllDirectories);
            Verbose.WriteLine($"Found {files.Length} *.sql files");

            // filter out SQL scripts to use
            var all = files
                .GroupBy(f => GetScriptGroupName(f, _targetPath))
                .Select(g => new ResetScript
                {
                    CategoryName = g.Key,
                    Files = g
                        .Select(f => new ResetScriptFile
                        {
                            FullPath = f,
                            BasePath = Path.GetRelativePath(_targetPath, f),
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
            var ignored = all.FirstOrDefault(i => i.CategoryName == null)?.Files;
            if (ignored.Count > 0)
            {
                Verbose.WriteLine($"Ignored {ignored.Count} files:");
                foreach (var item in ignored)
                {
                    Verbose.WriteLine($"- {item.BasePath}");
                }
            }

            if (scripts.Count == 0)
            {
                XConsole.NewPara().Warning.WriteLine("No SQL scripts found");
                XConsole.Write("No SQL scripts could be found at ").Cyan.WriteLine(_targetPath);
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
