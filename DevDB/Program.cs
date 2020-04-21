using System.Reflection;
using Atom.Util;

namespace DevDB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            XConsole.Yellow.WriteLine($"Version {Assembly.GetExecutingAssembly().GetName().Version}");
            XConsole.OK.WriteLine("Done");
        }
    }
}
