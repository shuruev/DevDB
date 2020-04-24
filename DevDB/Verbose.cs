using Atom.Util;

namespace DevDB
{
    public static class Verbose
    {
        public static bool Enabled { get; set; }

        public static void Write(string message = null)
        {
            if (!Enabled)
                return;

            XConsole.Muted.Write(message);
        }

        public static void WriteLine(string message = null)
        {
            if (!Enabled)
                return;

            XConsole.Muted.WriteLine(message);
        }
    }
}
