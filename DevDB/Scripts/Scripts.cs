using System;
using System.IO;
using System.Reflection;

namespace DevDB
{
    public static class Scripts
    {
        public static class Mssql
        {
            public static string DropAll => GetScript("Mssql.DropAll.sql");
            public static string DropSoft => GetScript("Mssql.DropSoft.sql");
            public static string GetTableCount => GetScript("Mssql.GetTableCount.sql");
            public static string GetProcedureCount => GetScript("Mssql.GetProcedureCount.sql");
        }

        private static string GetScript(string scriptName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var name = $"{assembly.GetName().Name}.Scripts.{scriptName}";

            using var stream = assembly.GetManifestResourceStream(name);
            if (stream == null)
                throw new InvalidOperationException($"Cannot find script '{scriptName}'");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
