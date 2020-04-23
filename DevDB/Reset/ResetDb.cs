using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Atom.Util;

namespace DevDB.Reset
{
    public partial class ResetDb
    {
        private readonly RunOptions _options;

        public ResetDb(RunOptions options)
        {
            _options = options;
        }

        public void Run()
        {
            switch (_options.DbType)
            {
                case DbType.Mssql:
                    RunMssql();
                    break;

                default:
                    throw new InvalidOperationException($"DB type {_options.DbType} is not supported yet");
            }
        }

        private void RunMssql()
        {
            XConsole.NewPara();

            if (_options.Connection == null)
            {
                XConsole.Warning.WriteLine("Connection is not set");
                XConsole.Write("Use -c to specify connection, e.g. ").Yellow.WriteLine("-c \"Server=localhost; Database=MyDb; Integrated Security=True;\"");
                return;
            }

            var csb = (SqlConnectionStringBuilder)_options.Connection;

            if (String.IsNullOrWhiteSpace(csb.DataSource))
            {
                XConsole.Warning.WriteLine("DB server is not set");
                XConsole.Write("Specify Server or Data Source location in your connection, e.g. -c \"").Yellow.Write("Server=localhost;").Default.Write(" Database=MyDb; ...\"");
                return;
            }

            if (String.IsNullOrWhiteSpace(csb.InitialCatalog))
            {
                XConsole.Warning.WriteLine("DB name is not set");
                XConsole.Write("Specify Database or Initial Catalog name in your connection, e.g. -c \"Server=localhost; ").Yellow.Write("Database=MyDb;").Default.Write(" ...\"");
                return;
            }

            XConsole.Write("Performing reset for ").Cyan.Write(csb.InitialCatalog).Default.Write(" database at ").Cyan.WriteLine(csb.DataSource);
            XConsole.Write("This will ").Error.Write("*** ERASE ***").Default.Write(" your local DB and recreate it from scripts. You sure (Y/N)? ");

            if (_options.AlwaysYes)
            {
                Console.Write('y');
            }
            else
            {
                var key = Console.ReadKey();
                if (key.Key != ConsoleKey.Y)
                    return;
            }

            XConsole.NewPara();
            MssqlRunReset();
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
