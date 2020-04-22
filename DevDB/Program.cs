using System;
using System.Reflection;
using Atom.Util;

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
                var run = ParseArguments(args);
                if (run == null)
                {
                    XConsole.NewPara()
                        .WriteLine("Usage:")
                        .Write("  devdb ").Yellow.Write("reset").Default.WriteLine(" <options>     Resets specified DB and recreates it from scripts")
                        .Write("  devdb ").Yellow.Write("migrate").Default.WriteLine(" <options>   Applies all available migrations to specified DB");
                }
                else
                {
                    switch (run.Command)
                    {
                        case RunCommand.Reset:
                            ResetDb(run);
                            break;

                        case RunCommand.Migrate:
                            MigrateDb(run);
                            break;

                        default:
                            throw new InvalidOperationException($"Unknown command: {run.Command}");
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

            //XConsole.PrintDemo();
            return 0;
        }

        private static string GetVersion()
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            if (ver.Revision == 0)
                return $"{ver.Major}.{ver.Minor}.{ver.Build}";

            return ver.ToString();
        }

        private static RunArgs ParseArguments(string[] args)
        {
            if (args == null)
                return null;

            if (args.Length < 2)
                return null;

            if (!Enum.TryParse<RunCommand>(args[0], true, out var cmd))
                return null;

            var run = new RunArgs { Command = cmd };

            // xxx populate other options

            return run;
        }

        private static void ResetDb(RunArgs args)
        {
            XConsole.NewPara().Yellow.WriteLine("RESET");
        }

        private static void MigrateDb(RunArgs args)
        {
            XConsole.NewPara().Yellow.WriteLine("MIGRATE");
        }
    }
}
