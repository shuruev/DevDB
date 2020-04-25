using System;
using System.Collections.Generic;
using System.Linq;

namespace DevDB.Reset
{
    public static class KnownScripts
    {
        private const string INIT = "Initial";
        private const string TABLES = "Tables";
        private const string VIEWS = "Views";
        private const string FUNCTIONS = "Functions";
        private const string PROCEDURES = "Procedures";
        private const string SECURITY = "Security";
        private const string OTHER = "Other";
        private const string DATA = "Data";

        private static readonly Dictionary<string, int> _order = new Dictionary<string, int>
        {
            { INIT, 1 },
            { TABLES, 2 },
            { VIEWS, 3 },
            { FUNCTIONS, 4 },
            { PROCEDURES, 5 },
            { SECURITY, 6 },
            { OTHER, 7 },
            { DATA, 8 }
        };

        private static readonly Dictionary<string, string> _aliases = new Dictionary<string, string>
        {
            { "ini", INIT },
            { "init", INIT },
            { "initial", INIT },
            { "initialize", INIT },
            { "setup", INIT },
            { "tb", TABLES },
            { "tab", TABLES },
            { "tbl", TABLES },
            { "table", TABLES },
            { "tables", TABLES },
            { "vw", VIEWS },
            { "view", VIEWS },
            { "views", VIEWS },
            { "fn", FUNCTIONS },
            { "fun", FUNCTIONS },
            { "func", FUNCTIONS },
            { "function", FUNCTIONS },
            { "functions", FUNCTIONS },
            { "sp", PROCEDURES },
            { "usp", PROCEDURES },
            { "prc", PROCEDURES },
            { "proc", PROCEDURES },
            { "procedure", PROCEDURES },
            { "procedures", PROCEDURES },
            { "usr", SECURITY },
            { "user", SECURITY },
            { "users", SECURITY },
            { "rol", SECURITY },
            { "role", SECURITY },
            { "roles", SECURITY },
            { "sec", SECURITY },
            { "security", SECURITY },
            { "oth", OTHER },
            { "other", OTHER },
            { "data", DATA },
            { "seeding", DATA }
        };

        static KnownScripts()
        {
            if (_aliases.Any(i => !_order.ContainsKey(i.Value)))
                throw new InvalidOperationException("Some aliases refer to unknown category");

            if (_order.Any(i => i.Key.Contains(" ")))
                throw new InvalidOperationException("Category name cannot contain whitespaces");
        }

        public static string Categorize(string groupName)
        {
            if (_aliases.TryGetValue(groupName.Trim().ToLower(), out var category))
                return category;

            return null;
        }

        public static int GetCategoryOrder(string categoryName)
        {
            return _order[categoryName];
        }
    }
}
