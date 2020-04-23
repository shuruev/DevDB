using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using Atom.Util;
using DevDB.Migrate;
using DevDB.Reset;

namespace DevDB
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            XConsole.NewLine()
                .Graphite.Write(" DevDB ")
                .Default.Write(" version ")
                .Green.WriteLine(GetVersion())
                .NewPara();

            try
            {
                var run = ParseArguments(args);
                if (run == null)
                {
                    XConsole.WriteLine("Usage:")
                        .Write("  devdb ").Yellow.Write("reset").Default.WriteLine(" <options>     Resets specified DB and recreates it from scripts")
                        .Write("  devdb ").Yellow.Write("migrate").Default.WriteLine(" <options>   Applies all available migrations to specified DB");
                }
                else
                {
                    switch (run.Command)
                    {
                        case RunCommand.Reset:
                            new ResetDb(run.Options).Run();
                            break;

                        case RunCommand.Migrate:
                            new MigrateDb(run.Options).Run();
                            break;

                        default:
                            throw new ArgumentException($"Unknown command: {run.Command}");
                    }
                }
            }
            catch (Exception e)
            {
                XConsole.NewPara().Error.WriteLine(e.Message);
                XConsole.Red.WriteLine(e.ToString());
                XConsole.PressAnyKeyWhenDebug();
                return -1;
            }

            return 0;
        }

        private static string GetVersion()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            if (ver.Revision == 0)
                return $"{ver.Major}.{ver.Minor}.{ver.Build}";

            return ver.ToString();
        }

        private static RunArgs ParseArguments(IReadOnlyList<string> args)
        {
            if (args == null)
                return null;

            if (args.Count < 1)
                return null;

            if (!TryParseEnum<RunCommand>(args[0], out var cmd))
                return null;

            var run = new RunArgs
            {
                Command = cmd,
                Options = ParseOptions(args.Skip(1).ToList())
            };

            return run;
        }

        private static RunOptions ParseOptions(List<string> args)
        {
            var options = new RunOptions();

            // read -v separately, since it affects logging
            if (args.Contains("-v"))
            {
                Verbose.Enabled = true;
                Verbose.WriteLine("Enabled verbose output");
            }

            // read -db separately, since it affects how other options work
            var db = args.FindIndex(i => i == "-db");
            if (db >= 0)
            {
                Verbose.WriteLine("Parsing DB type...");
                options.DbType = ParseDbType(args[db + 1]);
            }

            for (var i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                switch (arg)
                {
                    case "-v":
                        continue;

                    case "-db":
                        i += 1;
                        continue;

                    case "-c":
                        Verbose.WriteLine("Parsing connection...");
                        options.Connection = ParseConnection(options.DbType, args[i + 1]);
                        i += 1;
                        continue;

                    case "-p":
                        Verbose.WriteLine("Parsing path...");
                        options.CustomPath = ParsePath(args[i + 1]);
                        i += 1;
                        continue;

                    case "-y":
                        options.AlwaysYes = true;
                        continue;

                    default:
                        XConsole.Warning.Write("Unknown option:").Default.WriteLine($" {arg}");
                        break;
                }
            }

            return options;
        }

        private static bool TryParseEnum<T>(string arg, out T value)
        {
            value = default;

            var names = new HashSet<string>(Enum.GetNames(typeof(T)), StringComparer.OrdinalIgnoreCase);
            if (!names.Contains(arg))
                return false;

            value = (T)Enum.Parse(typeof(T), arg, true);
            return true;
        }

        private static DbType ParseDbType(string arg)
        {
            if (TryParseEnum<DbType>(arg, out var db))
                return db;

            throw new ArgumentException($"Unknown DB type: {arg}");
        }

        private static DbConnectionStringBuilder ParseConnection(DbType type, string arg)
        {
            switch (type)
            {
                case DbType.Mssql:
                    return ParseMssqlConnection(arg);

                default:
                    throw new InvalidOperationException($"DB type {type} is not supported yet");
            }
        }

        private static SqlConnectionStringBuilder ParseMssqlConnection(string arg)
        {
            try
            {
                return new SqlConnectionStringBuilder(arg);
            }
            catch
            {
                XConsole.Warning.Write("Invalid MSSQL connection string:").Default.WriteLine($" {arg}");
                throw;
            }
        }

        private static string ParsePath(string arg)
        {
            var path = Path.GetFullPath(arg);

            if (!Directory.Exists(path))
                throw new InvalidOperationException($"Invalid custom path: {path}");

            return path;
        }
    }
}
