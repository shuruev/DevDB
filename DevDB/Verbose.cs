using Atom.Util;

namespace DevDB
{
    public static class Verbose
    {
        public static bool Enabled { get; set; }

        public static void WriteLine(string message)
        {
            if (!Enabled)
                return;

            XConsole.Muted.WriteLine(message);
        }
    }
}
