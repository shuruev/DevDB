using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Atom.Util;

namespace DevDB.Other
{
    public class Help
    {
        private const string URL = "https://github.com/shuruev/DevDB#devdb";

        public void Run()
        {
            XConsole.NewPara().Write("Opening URL ").Cyan.Write(URL).Default.WriteLine("...");
            OpenBrowser(URL);
        }

        private static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
                return;
            }

            throw new InvalidOperationException("Unknown platform, not sure how to open URL in default browser");
        }
    }
}
