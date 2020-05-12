using System;
using System.Collections.Generic;
using System.Linq;

namespace DevDB.Reset
{
    public static class KnownScripts
    {
        private const string INIT = "Initial";
        private const string TYPES = "Types";
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
            { TYPES, 2 },
            { TABLES, 3 },
            { VIEWS, 4 },
            { FUNCTIONS, 5 },
            { PROCEDURES, 6 },
            { SECURITY, 7 },
            { OTHER, 8 },
            { DATA, 9 }
        };

        private static readonly Dictionary<string, string> _aliases = new Dictionary<string, string>
        {
            { "ini", INIT },
            { "init", INIT },
            { "initial", INIT },
            { "initialize", INIT },
            { "setup", INIT },
            { "typ", TYPES },
            { "type", TYPES },
            { "types", TYPES },
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
            { "post", OTHER },
            { "data", DATA },
            { "seed", DATA },
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

        public static bool UsedWhenSoftReset(string categoryName)
        {
            switch (categoryName)
            {
                case TYPES:
                case VIEWS:
                case FUNCTIONS:
                case PROCEDURES:
                    return true;

                default:
                    return false;
            }
        }
    }
}
