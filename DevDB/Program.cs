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
using Npgsql;

namespace DevDB
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            XConsole.NewLine()
                .Graphite.Write(" DevDB ")
                .Default.Write(" version ")
                .Green.WriteLine(GetVersion());

            try
            {
                XConsole.NewPara();
                var run = ParseArguments(args);
                if (run == null)
                {
                    XConsole.WriteLine("Usage:")
                        .Write("  devdb ").Yellow.Write("reset").Default.WriteLine(" <CONNECTION_STRING> [options]     Resets specified DB and recreates it from scripts")
                        .Write("  devdb ").Yellow.Write("migrate").Default.WriteLine(" <CONNECTION_STRING> [options]   Applies all available migrations to specified DB");

                    XConsole.NewPara().WriteLine("Options:")
                        .WriteLine("  -db <DB_TYPE>      Specifies DB engine type: 'mssql' or 'pgsql' (default is 'mssql')")
                        .WriteLine("  -p <TARGET_PATH>   Specifies custom target path (current folder will be used by default)")
                        .WriteLine("  -y                 Automatically submits 'yes' to all Y/N prompts")
                        .WriteLine("  -v                 Enables verbose output");
                }
                else
                {
                    XConsole.NewPara();
                    var ctx = InitializeContext(run);
                    if (ctx != null)
                    {
                        switch (run.Command)
                        {
                            case RunCommand.Reset:
                                new ResetDb(ctx).Run();
                                break;

                            case RunCommand.Migrate:
                                new MigrateDb(ctx).Run();
                                break;

                            default:
                                throw new ArgumentException($"Unknown command: {run.Command}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                XConsole.NewPara().Error.WriteLine(e.Message);
                XConsole.Red.WriteLine(e.ToString());
                XConsole.PressAnyKey();
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

        private static RunContext InitializeContext(RunArgs run)
        {
            // init DB connection
            var ctx = new RunContext
            {
                DbType = run.DbType,
                Connection = run.Connection
            };

            if (ctx.Connection != null)
            {
                Verbose.WriteLine($"DB type: {ctx.DbType.ToString().ToUpper()}");
                Verbose.WriteLine($"DB connection: {ctx.Connection}");
            }

            // choose target path
            ctx.TargetPath = Environment.CurrentDirectory;
            if (!String.IsNullOrWhiteSpace(run.CustomPath))
                ctx.TargetPath = run.CustomPath;

            if (!Directory.Exists(ctx.TargetPath))
            {
                XConsole.NewPara().Warning.WriteLine("Cannot locate target path");
                XConsole.Write("The following target path was not found: ").Cyan.WriteLine(ctx.TargetPath);
                XConsole.Write("When you use ").Yellow.Write("-p").Default.WriteLine(" parameter to specify custom path, make sure it points to existing location");
                return null;
            }

            Verbose.WriteLine($"Target path: {ctx.TargetPath}");

            // get log path
            ctx.LogPath = Path.Combine(ctx.TargetPath, "log");
            Verbose.Write($"Log path: {ctx.LogPath}");

            if (!Directory.Exists(ctx.LogPath))
            {
                Directory.CreateDirectory(ctx.LogPath);
                Verbose.Write(" (created)");
            }

            Verbose.WriteLine();

            // optionally disable prompts

            return ctx;
        }

        private static RunArgs ParseArguments(IEnumerable<string> arguments)
        {
            if (arguments == null)
                return null;

            var args = arguments.ToList();
            if (args.Count < 1)
                return null;

            // parse command name
            if (!TryParseEnum<RunCommand>(args[0], out var cmd))
                return null;

            var run = new RunArgs { Command = cmd };
            args.RemoveAt(0);

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
                run.DbType = ParseDbType(args[db + 1]);
            }

            // parse required arguments based on command
            switch (run.Command)
            {
                case RunCommand.Reset:
                    if (args.Count > 0 && !args[0].StartsWith("-"))
                    {
                        Verbose.WriteLine("Parsing connection...");
                        run.Connection = ParseConnection(run.DbType, args[0]);
                        args.RemoveAt(0);
                    }
                    break;
            }

            // parse all options
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

                    case "-p":
                        Verbose.WriteLine("Parsing custom path...");
                        run.CustomPath = ParsePath(args[i + 1]);
                        i += 1;
                        continue;

                    case "-y":
                        Prompt.AlwaysYes = true;
                        Verbose.WriteLine("Prompt will be disabled");
                        continue;

                    default:
                        XConsole.Warning.Write("Unknown option:").Default.WriteLine($" {arg}");
                        break;
                }
            }

            return run;
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

                case DbType.Pgsql:
                    return ParsePgsqlConnection(arg);

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
                XConsole.Warning.Write("Invalid SQL Server connection string:").Default.WriteLine($" {arg}");
                throw;
            }
        }

        private static NpgsqlConnectionStringBuilder ParsePgsqlConnection(string arg)
        {
            try
            {
                return new NpgsqlConnectionStringBuilder(arg);
            }
            catch
            {
                XConsole.Warning.Write("Invalid PostgreSQL connection string:").Default.WriteLine($" {arg}");
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
