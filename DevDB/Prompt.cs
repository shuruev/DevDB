using System;

namespace DevDB
{
    public static class Prompt
    {
        public static bool AlwaysYes { get; set; }

        public static bool YesNo()
        {
            if (AlwaysYes)
                return true;

            var key = Console.ReadKey(true);
            return key.Key == ConsoleKey.Y
                || key.Key == ConsoleKey.D1
                || key.Key == ConsoleKey.NumPad1;
        }
    }
}
